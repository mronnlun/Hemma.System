using Hemma.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Hemma.Web.Controllers
{
    public class NibeDataController : ApiController
    {
        public List<List<object>> GetChartData(int millisecondOffset, string id)
        {
            return GetChartData(DateTime.Now.AddMilliseconds(millisecondOffset * -1), DateTime.Now, id);
        }

        private List<List<object>> GetChartData(DateTime startDate, DateTime endDate, string id)
        {
            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase("Nibe");

            var collection = database.GetCollection<NibeData>("Data");

            var data = from item in collection.AsQueryable()
                       where item.Timestamp > startDate.Ticks && item.Timestamp < endDate.Ticks
                       select new
                       {
                           item.Timestamp,
                           item.Utetemperatur
                       };

            var selectedItems = new List<List<object>>();
            foreach (var item in data.ToList())
            {
                selectedItems.Add(new List<object>() { new DateTime(item.Timestamp), item.Utetemperatur });
            }

            return selectedItems;

        }


        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}