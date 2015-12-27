using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hemma.Web.Controllers
{
    public class TemperaturController : Controller
    {
        // GET: Temperatur
        public ActionResult Index()
        {
            ViewBag.ChannelId = ConfigurationManager.AppSettings["Temperature:ChannelId"];
            ViewBag.ApiReadKey = ConfigurationManager.AppSettings["Temperature:ApiReadyKey"];
            ViewBag.DataInterval = ConfigurationManager.AppSettings["Temperature:DataInterval"];
            return View();
        }
    }
}