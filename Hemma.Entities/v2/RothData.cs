using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hemma.Entities.v2
{
    public class RothData : IData
    {
        public DateTime Datestamp { get; set; }
        public long Timestamp { get; set; }

        public double Arbetsrum { get; set; }
        public double ArbetsrumSetting { get; set; }

        public double Vardagsrum { get; set; }
        public double VardagsrumSetting { get; set; }

        public double Hallen { get; set; }
        public double HallenSetting { get; set; }

        public double Lekrum { get; set; }
        public double LekrumSetting { get; set; }

        public double MammasPappas { get; set; }
        public double MammasPappasSetting { get; set; }

        public double Viggo { get; set; }
        public double ViggoSetting { get; set; }

        public double Felix { get; set; }
        public double FelixSetting { get; set; }

    }
}
