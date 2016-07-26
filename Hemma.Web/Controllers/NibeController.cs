using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hemma.Web.Controllers
{
    public class NibeController : Controller
    {
        public IEnumerable<object> Get(string databaseName)
        {
            //TODO Check that collectionName is among allowed collections
            var mongoclient = new MongoClient(new MongoUrl("mongodb://hemmaserver2"));
            var database = mongoclient.GetDatabase(databaseName);

            var collection = database.GetCollection<object>("Data");

            var data = collection.AsQueryable().Take(20).ToList();

            return data;
        }

        // GET: Nibe
        public ActionResult Index()
        {
            return View();
        }

    }
}