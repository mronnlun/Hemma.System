using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hemma.Interfaces
{
    public interface INibeData
    {
        [BsonId]
        long Timestamp { get; set; }

        [NibeField(Id = "40004")]
        decimal Utetemperatur { get; set; }
        [NibeField(Id = "43084")]
        decimal EffektEltillsats { get; set; }
        [NibeField(Id = "43081")]
        decimal Tidfaktor { get; set; }
        [NibeField(Id = "43420")]
        decimal KompressorDifttid { get; set; }
        [NibeField(Id = "43424")]
        decimal KompressorDrifttidVarmvatten { get; set; }
        [NibeField(Id = "43416")]
        int Kompressorstarter { get; set; }
        [NibeField(Id = "43005")]
        decimal Gradminuter { get; set; }
        [NibeField(Id = "40014")]
        decimal VarmvattenLaddning { get; set; }
        [NibeField(Id = "40013")]
        decimal VarmvattenTopp { get; set; }
        [NibeField(Id = "40015")]
        decimal KöldbärareIn { get; set; }
        [NibeField(Id = "40016")]
        decimal KöldbärareUt { get; set; }
        [NibeField(Id = "43439")]
        decimal KöldbärarPumphastiget { get; set; }
        [NibeField(Id = "43437")]
        decimal VärmebärarPumphastiget { get; set; }
        [NibeField(Id = "40008")]
        decimal InneFramledningstemp { get; set; }
        [NibeField(Id = "40012")]
        decimal InneReturtemp { get; set; }
        [NibeField(Id = "43009")]
        decimal InneBeräknadFramledningstemp { get; set; }
        [NibeField(Id = "40007")]
        decimal GarageFramledningstemp { get; set; }
        [NibeField(Id = "40129")]
        decimal GarageReturtemp { get; set; }
        [NibeField(Id = "43008")]
        decimal GarageBeräknadFramledningstemp { get; set; }
        [NibeField(Id = "40018")]
        decimal Hetgas { get; set; }
        [NibeField(Id = "40017")]
        decimal KondensorFram { get; set; }
        [NibeField(Id = "40022")]
        decimal Suggas { get; set; }
        [NibeField(Id = "40019")]
        decimal Vätskeledning { get; set; }
    }

}
