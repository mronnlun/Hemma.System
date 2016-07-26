using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hemma.Entities.v1
{
    public class RothData : IData
    {
        [BsonId]
        public long Timestamp { get; set; }

        public decimal Arbetsrum { get; set; }
        public decimal ArbetsrumSetting { get; set; }

        public decimal Vardagsrum { get; set; }
        public decimal VardagsrumSetting { get; set; }

        public decimal Hallen { get; set; }
        public decimal HallenSetting { get; set; }

        public decimal Lekrum { get; set; }
        public decimal LekrumSetting { get; set; }

        public decimal MammasPappas { get; set; }
        public decimal MammasPappasSetting { get; set; }

        public decimal Viggo { get; set; }
        public decimal ViggoSetting { get; set; }

        public decimal Felix { get; set; }
        public decimal FelixSetting { get; set; }

    }
}
