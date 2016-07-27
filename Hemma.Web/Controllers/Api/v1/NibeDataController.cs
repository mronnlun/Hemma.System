using Hemma.Entities;
using Hemma.Entities.v2;
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

    public class NibeDataController : ApiController
    {
        [ArrayInput("fields")]
        public IEnumerable<IEnumerable<object>> GetChartData(int millisecondOffset, string[] fields)
        {
            return GetChartData(DateTime.Now.AddMilliseconds(millisecondOffset * -1), DateTime.Now, fields);
        }

        internal double GetLatestValue(string id)
        {
            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase("Nibe");

            var collection = database.GetCollection<NibeData>("LoggedData");

            var last = collection.AsQueryable().OrderByDescending(item => item.Timestamp).Take(20);
            return last.First().Utetemperatur;

        }

        static PropertyInfo[] fieldAccessors = typeof(NibeData).GetProperties();

        private IEnumerable<IEnumerable<object>> GetChartData(DateTime startDate, DateTime endDate, string[] fields)
        {
            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase("Nibe");

            var collection = database.GetCollection<NibeData>("LoggedData");

            var data = from item in collection.AsQueryable()
                       where item.Timestamp > startDate.Ticks && item.Timestamp < endDate.Ticks
                       select new List<object>() { item.Timestamp, item.Utetemperatur };

            return data.ToList();

        }



    }
}