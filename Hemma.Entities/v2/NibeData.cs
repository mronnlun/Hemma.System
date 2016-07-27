using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hemma.Entities.v2
{
    public class NibeData : IData
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public DateTime Datestamp { get; set; }
        public long Timestamp { get; set; }

        [NibeField(Id = "40004")]
        public double Utetemperatur { get; set; }
        [NibeField(Id = "43084")]
        public double EffektEltillsats { get; set; }
        [NibeField(Id = "43081")]
        public double Tidfaktor { get; set; }
        [NibeField(Id = "43420")]
        public double KompressorDifttid { get; set; }
        [NibeField(Id = "43424")]
        public double KompressorDrifttidVarmvatten { get; set; }
        [NibeField(Id = "43416")]
        public int Kompressorstarter { get; set; }
        [NibeField(Id = "43005")]
        public double Gradminuter { get; set; }
        [NibeField(Id = "40014")]
        public double VarmvattenLaddning { get; set; }
        [NibeField(Id = "40013")]
        public double VarmvattenTopp { get; set; }
        [NibeField(Id = "40015")]
        public double KöldbärareIn { get; set; }
        [NibeField(Id = "40016")]
        public double KöldbärareUt { get; set; }
        [NibeField(Id = "43439")]
        public double KöldbärarPumphastiget { get; set; }
        [NibeField(Id = "43437")]
        public double VärmebärarPumphastiget { get; set; }
        [NibeField(Id = "40008")]
        public double InneFramledningstemp { get; set; }
        [NibeField(Id = "40012")]
        public double InneReturtemp { get; set; }
        [NibeField(Id = "43009")]
        public double InneBeräknadFramledningstemp { get; set; }
        [NibeField(Id = "40007")]
        public double GarageFramledningstemp { get; set; }
        [NibeField(Id = "40129")]
        public double GarageReturtemp { get; set; }
        [NibeField(Id = "43008")]
        public double GarageBeräknadFramledningstemp { get; set; }
        [NibeField(Id = "40018")]
        public double Hetgas { get; set; }
        [NibeField(Id = "40017")]
        public double KondensorFram { get; set; }
        [NibeField(Id = "40022")]
        public double Suggas { get; set; }
        [NibeField(Id = "40019")]
        public double Vätskeledning { get; set; }
    }

}
