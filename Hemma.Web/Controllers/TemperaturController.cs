﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hemma.Web.Controllers
{
    public class TemperaturController : Controller
    {
        public ActionResult Mathias()
        {
            return Temperatur("mathias");
        }

        public ActionResult Kaj()
        {
            return Temperatur("kaj");
        }

        ActionResult Temperatur(string id)
        {
            ViewBag.ChannelId = ConfigurationManager.AppSettings["Temperature:" + id.ToLower() + ":ChannelId"];
            ViewBag.ApiReadKey = ConfigurationManager.AppSettings["Temperature:" + id.ToLower() + ":ApiReadyKey"];
            ViewBag.DataInterval = ConfigurationManager.AppSettings["Temperature:" + id.ToLower() + ":DataInterval"];
            return View("Index");

        }
    }
}