﻿using System;
using System.Collections.Generic;
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
            return View();
        }
    }
}