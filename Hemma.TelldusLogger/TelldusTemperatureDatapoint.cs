using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hemma.TelldusLogger
{
    public class TelldusTemperatureDatapoint
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public DateTime Datestamp { get; set; }
        public long Timestamp { get; set; }

        public string Name { get; set; }
        public double Value { get; set; }
        public double Temperature { get; internal set; }
        public double Humidity { get; internal set; }

        public int Battery { get; set; }
    }
}
