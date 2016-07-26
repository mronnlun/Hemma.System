using Hemma.Entities;
using Hemma.Entities.v2;
using HtmlAgilityPack;
using Microsoft.Win32;
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

            var registryContainer = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Nanocon\\");
            var correctusername = registryContainer.GetValue("NibeUsername")?.ToString();
            var correctpassword = registryContainer.GetValue("NibePassword")?.ToString();

            using (var webclient = new CookieAwareWebClient())
            {
                var values = new NameValueCollection { { "Email", correctusername },
                    { "Password", correctpassword }
                };

                webclient.Encoding = Encoding.UTF8;
                webclient.DownloadString("https://www.nibeuplink.com/");

                webclient.UploadValues(new Uri("https://www.nibeuplink.com/LogIn"), "POST", values);

                rawHtml =
                    webclient.DownloadString("https://www.nibeuplink.com/System/18582/Status/ServiceInfo");
            }

            var nibeData = new NibeData();
            var now = DateTime.Now;
            nibeData.Timestamp = now.Ticks;
            nibeData.Datestamp = now;

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
                    case "double":
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

            var datas = database.GetCollection<NibeData>("LoggedData");
            datas.InsertOne(nibeData);


            //var oldDataColl = database.GetCollection<Hemma.Entities.v1.NibeData>("Data");
            //var oldDatas = oldDataColl.AsQueryable().ToList();
            //var newDatas = new List<NibeData>();
            //foreach (var oldItem in oldDatas)
            //{
            //    var newdata = new NibeData();
            //    var stamp = new DateTime(oldItem.Timestamp);
            //    newdata.Datestamp = stamp;
            //    newdata.EffektEltillsats = Convert.ToDouble(oldItem.EffektEltillsats);
            //    newdata.GarageBeräknadFramledningstemp = Convert.ToDouble(oldItem.GarageBeräknadFramledningstemp);
            //    newdata.GarageFramledningstemp = Convert.ToDouble(oldItem.GarageFramledningstemp);
            //    newdata.GarageReturtemp = Convert.ToDouble(oldItem.GarageReturtemp);
            //    newdata.Gradminuter = Convert.ToDouble(oldItem.Gradminuter);
            //    newdata.Hetgas = Convert.ToDouble(oldItem.Hetgas);
            //    newdata.InneBeräknadFramledningstemp = Convert.ToDouble(oldItem.InneBeräknadFramledningstemp);
            //    newdata.InneFramledningstemp = Convert.ToDouble(oldItem.InneFramledningstemp);
            //    newdata.InneReturtemp = Convert.ToDouble(oldItem.InneReturtemp);
            //    newdata.KompressorDifttid = Convert.ToDouble(oldItem.KompressorDifttid);
            //    newdata.KompressorDrifttidVarmvatten = Convert.ToDouble(oldItem.KompressorDrifttidVarmvatten);
            //    newdata.Kompressorstarter = oldItem.Kompressorstarter;
            //    newdata.KondensorFram = Convert.ToDouble(oldItem.KondensorFram);
            //    newdata.KöldbärareIn = Convert.ToDouble(oldItem.KöldbärareIn);
            //    newdata.KöldbärareUt = Convert.ToDouble(oldItem.KöldbärareUt);
            //    newdata.KöldbärarPumphastiget = Convert.ToDouble(oldItem.KöldbärarPumphastiget);
            //    newdata.Suggas = Convert.ToDouble(oldItem.Suggas);
            //    newdata.Tidfaktor = Convert.ToDouble(oldItem.Tidfaktor);
            //    newdata.Timestamp = oldItem.Timestamp;
            //    newdata.Utetemperatur = Convert.ToDouble(oldItem.Utetemperatur);
            //    newdata.VarmvattenLaddning = Convert.ToDouble(oldItem.VarmvattenLaddning);
            //    newdata.VarmvattenTopp = Convert.ToDouble(oldItem.VarmvattenTopp);
            //    newdata.VärmebärarPumphastiget = Convert.ToDouble(oldItem.VärmebärarPumphastiget);
            //    newdata.Vätskeledning = Convert.ToDouble(oldItem.Vätskeledning);
            //    newDatas.Add(newdata);
            //}
            //datas.InsertMany(newDatas);

        }

        static char[] allowedChars = "0123456789,.-".ToCharArray();

        private static double GetNumber(string input)
        {
            var parsedInput = new string(input.Where(c => allowedChars.Contains(c)).ToArray()).Replace(".", ",");
            double number;
            if (double.TryParse(parsedInput, out number))
                return number;
            else
                return 0;
        }

    }
}
