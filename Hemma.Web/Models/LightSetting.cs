using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Hemma.Web.Models
{
    public class LightSetting
    {
        public LightSetting()
        {
            this.CanTurnOn = true;
        }

        public string Room { get; set; }
        public string Lampa { get; set; }

        public string AlexaName { get; set; }

        static string replace = "å,a;ä,a;ö,o;Å,A;Ä,A;Ö,O; ,_;(,_;),_";

        static Dictionary<string, string> replaceList = GetReplaceDictionary();

        static Dictionary<string, string> GetReplaceDictionary()
        {
            var replaceGroups = new Dictionary<string, string>();

            var groups = replace.Split(';');
            foreach (var group in groups)
            {
                var values = group.Split(',');
                replaceGroups.Add(values[0], values[1]);
            }

            return replaceGroups;
        }


        string GetNormalizedValue(string value)
        {

            foreach (var item in replaceList)
            {
                value = value.Replace(item.Key, item.Value);
            }

            return value;
        }

        string _roomNormalized = null;
        public string RoomNormalized
        {
            get
            {
                if (_roomNormalized == null)
                    _roomNormalized = GetNormalizedValue(this.Room);

                return _roomNormalized;
            }
        }

        string _lampaNormalized = null;
        public string LampaNormalized
        {
            get
            {
                if (_lampaNormalized == null)
                    _lampaNormalized = GetNormalizedValue(this.Lampa);

                return _lampaNormalized;
            }
        }

        [DefaultValue(true)]
        public bool CanTurnOn { get; set; }

        public List<LightAddress> Addresses { get; set; }
        public string StatusAddress { get; set; }
    }

    public class LightAddress
    {
        public string Address { get; set; }
        public int OnValue { get; set; }
    }
}
