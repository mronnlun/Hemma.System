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
        // GET: Nibe
        public ActionResult Index()
        {
            return View();
        }

    }
}