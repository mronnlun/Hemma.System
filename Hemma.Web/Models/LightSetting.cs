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
