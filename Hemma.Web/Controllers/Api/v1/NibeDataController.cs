using Hemma.Entities;
using Hemma.Entities.v1;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace Hemma.Web.Controllers.Api.v1
{
    public class test
    {
        public int Intx { get; set; }
        public long Longx { get; set; }
        public string Stringx { get; set; }
        public DateTime Datex { get; set; }
        public decimal Decix { get; set; }
        public double Doublex { get; set; }
        public bool Boolx { get; internal set; }
        public Int64 Int64x { get; set; }
        public UInt64 UInt64x { get; set; }
    }
    public class NibeDataController : ApiController
    {
        [ArrayInput("fields")]
        public IEnumerable<IEnumerable<object>> GetChartData(int millisecondOffset, string[] fields)
        {
            return GetChartData(DateTime.Now.AddMilliseconds(millisecondOffset * -1), DateTime.Now, fields);
        }

        public object Get20()
        {
            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase("Nibe");

            var collection = database.GetCollection<NibeData>("Data");

            var data = collection.AsQueryable().Take(20).ToList();

            var x = new test();
            x.Datex = DateTime.UtcNow;
            x.Decix = 4.5M;
            x.Doublex = 5.6;
            x.Intx = 7;
            x.Longx = 8;
            x.Stringx = "abc";
            x.Boolx = true;
            x.Int64x = 8;
            x.UInt64x = 9;

            //database.CreateCollection("x");
            var xcol = database.GetCollection<test>("x");
            xcol.InsertOne(x);

            return data;
        }

        internal decimal GetLatestValue(string id)
        {
            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase("Nibe");

            var collection = database.GetCollection<NibeData>("Data");

            return collection.AsQueryable().OrderByDescending(item => item.Timestamp).FirstOrDefault().Utetemperatur;

        }

        static PropertyInfo[] fieldAccessors = typeof(NibeData).GetProperties();

        private IEnumerable<IEnumerable<object>> GetChartData(DateTime startDate, DateTime endDate, string[] fields)
        {
            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase("Nibe");

            var collection = database.GetCollection<NibeData>("Data");

            var data = from item in collection.AsQueryable()
                       where item.Timestamp > startDate.Ticks && item.Timestamp < endDate.Ticks
                       select new List<object>() { item.Timestamp, item.Utetemperatur };

            return data.ToList();

        }



    }
}