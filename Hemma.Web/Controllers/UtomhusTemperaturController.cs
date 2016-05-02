using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hemma.Web.Controllers
{
    public class UtomhusTemperaturController : Controller
    {
        // GET: UtomhusTemperatur
        public ActionResult Index()
        {
            var data = new NibeDataController();
            decimal value = data.GetLatestValue("utetemperatur");
            return View(value);
        }
    }
}