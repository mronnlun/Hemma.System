using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hemma.Entities
{
    public class NibeData
    {
        [BsonId]
        public long Timestamp { get; set; }

        [NibeField(Id = "40004")]
        public decimal Utetemperatur { get; set; }
        [NibeField(Id = "43084")]
        public decimal EffektEltillsats { get; set; }
        [NibeField(Id = "43081")]
        public decimal Tidfaktor { get; set; }
        [NibeField(Id = "43420")]
        public decimal KompressorDifttid { get; set; }
        [NibeField(Id = "43424")]
        public decimal KompressorDrifttidVarmvatten { get; set; }
        [NibeField(Id = "43416")]
        public int Kompressorstarter { get; set; }
        [NibeField(Id = "43005")]
        public decimal Gradminuter { get; set; }
        [NibeField(Id = "40014")]
        public decimal VarmvattenLaddning { get; set; }
        [NibeField(Id = "40013")]
        public decimal VarmvattenTopp { get; set; }
        [NibeField(Id = "40015")]
        public decimal KöldbärareIn { get; set; }
        [NibeField(Id = "40016")]
        public decimal KöldbärareUt { get; set; }
        [NibeField(Id = "43439")]
        public decimal KöldbärarPumphastiget { get; set; }
        [NibeField(Id = "43437")]
        public decimal VärmebärarPumphastiget { get; set; }
        [NibeField(Id = "40008")]
        public decimal InneFramledningstemp { get; set; }
        [NibeField(Id = "40012")]
        public decimal InneReturtemp { get; set; }
        [NibeField(Id = "43009")]
        public decimal InneBeräknadFramledningstemp { get; set; }
        [NibeField(Id = "40007")]
        public decimal GarageFramledningstemp { get; set; }
        [NibeField(Id = "40129")]
        public decimal GarageReturtemp { get; set; }
        [NibeField(Id = "43008")]
        public decimal GarageBeräknadFramledningstemp { get; set; }
        [NibeField(Id = "40018")]
        public decimal Hetgas { get; set; }
        [NibeField(Id = "40017")]
        public decimal KondensorFram { get; set; }
        [NibeField(Id = "40022")]
        public decimal Suggas { get; set; }
        [NibeField(Id = "40019")]
        public decimal Vätskeledning { get; set; }
    }

}
