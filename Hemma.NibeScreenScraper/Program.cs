using Hemma.Entities;
using HtmlAgilityPack;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hemma.NibeScreenScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            var rawHtml = "";

            using (var webclient = new CookieAwareWebClient())
            {
                var values = new NameValueCollection { { "Email", "mathias.ronnlund@gmail.com" },
                    { "Password", "" }
                };

                webclient.Encoding = Encoding.UTF8;
                webclient.DownloadString("https://www.nibeuplink.com/");

                webclient.UploadValues(new Uri("https://www.nibeuplink.com/LogIn"), "POST", values);

                rawHtml =
                    webclient.DownloadString("https://www.nibeuplink.com/System/18582/Status/ServiceInfo");
            }

            var nibeData = new NibeData();
            nibeData.Timestamp = DateTime.Now.Ticks;
            var dataProperties = new Dictionary<string, PropertyInfo>();
            foreach (var property in nibeData.GetType().GetProperties())
            {
                var attributes = property.GetCustomAttributes<NibeFieldAttribute>();
                if (attributes.Count() > 0)
                {
                    var nibeField = attributes.ElementAt(0) as NibeFieldAttribute;
                    dataProperties.Add(nibeField.Id, property);
                }
            }

            var html = new HtmlDocument();
            html.LoadHtml(rawHtml);
            var root = html.DocumentNode;
            var trRows = root.Descendants().Where(n =>
            n.Name == "tr" && n.Descendants().Where(span =>
              span.Name == "span" && span.GetAttributeValue("class", "").Contains("AutoUpdateValue")).Count() > 0);
            foreach (var tr in trRows)
            {
                var tds = tr.Descendants().Where(element => element.Name == "td");
                var name = tds.ElementAt(0).InnerText.Trim();
                var value = tds.ElementAt(1).InnerText.Trim();
                var number = GetNumber(value);
                var fieldIdContainer = tds.ElementAt(1).Descendants("span").ElementAt(0).GetAttributeValue("class", "").Split(' ');
                var fieldId = "";
                if (fieldIdContainer.Length > 0)
                    fieldId = fieldIdContainer.ElementAt(1);

                var dataProperty = dataProperties.FirstOrDefault(item => fieldId.Contains(item.Key));

                if (dataProperty.Key == null)
                    continue;

                switch (dataProperty.Value.PropertyType.Name.ToLower())
                {
                    case "string":
                        break;
                    case "decimal":
                        dataProperty.Value.SetValue(nibeData, number);
                        break;
                    case "int32":
                        dataProperty.Value.SetValue(nibeData, (int)number);
                        break;
                    default:
                        break;
                }

            }


            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase("Nibe");

            var datas = database.GetCollection<NibeData>("Data");
            datas.InsertOne(nibeData);
        }

        static char[] allowedChars = "0123456789,.-".ToCharArray();

        private static decimal GetNumber(string input)
        {
            var parsedInput = new string(input.Where(c => allowedChars.Contains(c)).ToArray()).Replace(".", ",");
            decimal number;
            if (decimal.TryParse(parsedInput, out number))
                return number;
            else
                return 0;
        }

    }
}
