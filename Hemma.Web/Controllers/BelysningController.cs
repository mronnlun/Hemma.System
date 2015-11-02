using KNXLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Hemma.Web.Controllers
{
    public class BelysningController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View(settings);
        }

        static IEnumerable<Setting> settings = GetSettings();

        private static IEnumerable<Setting> GetSettings()
        {
            var settings = new List<Setting>();
            settings.Add(new Setting()
            {
                Room = "Arbetsrum",
                Lampa = "Taklampa",
                Adresser = new List<string>() { "1/0/9", "1/0/8" },
                StatusAddress = "1/0/9"

            });

            settings.Add(new Setting()
            {
                Room = "Kök",
                Lampa = "Bänk",
                Adresser = new List<string>() { "1/0/5", "1/0/4" },
                StatusAddress = "1/0/5"
            });

            settings.Add(new Setting()
            {
                Room = "Kök",
                Lampa = "Matplats",
                Adresser = new List<string>() { "1/1/8" },
                StatusAddress = "1/1/8"
            });

            settings.Add(new Setting()
            {
                Room = "Utebelysning",
                Lampa = "Baksida",
                Adresser = new List<string>() { "1/0/16", "1/0/17" },
                StatusAddress = "1/0/17"
            });
            settings.Add(new Setting()
            {
                Room = "Utebelysning",
                Lampa = "Terass",
                Adresser = new List<string>() { "1/0/10", "1/0/11" },
                StatusAddress = "1/0/11"
            });

            return settings;
        }

        private static readonly object knxLock = new object();

        [Authorize]
        [HttpGet]
        public void UpdateLightStatus()
        {
            lock (knxLock)
            {
                var localAddress = ConfigurationManager.AppSettings["localAddress"];
                var localPort = new Random().Next(1500, 5000);
                var remoteAddress = ConfigurationManager.AppSettings["remoteAddress"];
                var remotePort = int.Parse(ConfigurationManager.AppSettings["remotePort"]);

                var connection = new KnxConnectionTunneling(remoteAddress, remotePort, localAddress, localPort);
                try
                {
                    connection.KnxStatusDelegate += ReceiveKnxStatus;
                    connection.Connect();
                    Thread.Sleep(500);
                    foreach (var setting in GetSettings())
                    {
                        connection.RequestStatus(setting.StatusAddress);
                        Thread.Sleep(100);
                    }
                    //Wait 5 seconds so statuses can update
                    Thread.Sleep(5000);
                }
                finally
                {
                    if (connection != null)
                        connection.Disconnect();
                }
            }

        }

        [Authorize]
        [HttpPost]
        public JsonResult GetLightStatus(string room, string lampa)
        {
            var setting = settings.FirstOrDefault(item => item.Room.Equals(room, StringComparison.CurrentCultureIgnoreCase) &&
                item.Lampa.Equals(lampa, StringComparison.CurrentCultureIgnoreCase));

            string state = null;
            if (setting != null)
            {

                var until = DateTime.Now.AddSeconds(5);
                while (DateTime.Now < until)
                {
                    state = HttpContext.Application["LightStatus_" + setting.StatusAddress] as string;
                    if (state != null)
                        break;
                }
            }

            return Json(new
            {
                room = room,
                lampa = lampa,
                state = (!string.IsNullOrWhiteSpace(state) ? state : "off")
            });
        }


        private void ReceiveKnxStatus(string address, byte[] state)
        {
            if (state != null && state.Length > 0 && state[0] == 1)
                HttpContext.Application["LightStatus_" + address] = "on";
            else
                HttpContext.Application["LightStatus_" + address] = "off";
        }

        [Authorize]
        [HttpPost]
        public JsonResult Ändra(string room, string lampa, string state)
        {
            var setting = settings.FirstOrDefault(item => item.Room.Equals(room, StringComparison.CurrentCultureIgnoreCase) &&
                item.Lampa.Equals(lampa, StringComparison.CurrentCultureIgnoreCase));
            if (setting != null)
            {
                lock (knxLock)
                {

                    var localAddress = ConfigurationManager.AppSettings["localAddress"];
                    var localPort = new Random().Next(1500, 5000);
                    var remoteAddress = ConfigurationManager.AppSettings["remoteAddress"];
                    var remotePort = int.Parse(ConfigurationManager.AppSettings["remotePort"]);

                    var connection = new KnxConnectionTunneling(remoteAddress, remotePort, localAddress, localPort);
                    try
                    {
                        connection.KnxConnectedDelegate += KnxConnected;
                        connection.KnxEventDelegate += KnxEvent;
                        connection.KnxStatusDelegate += KnxStatusEvent;
                        connection.Connect();
                        Thread.Sleep(100);
                        var value = (state.ToLower() == "on" ? true : false);
                        foreach (var adress in setting.Adresser)
                            connection.Action(adress, value);
                        Thread.Sleep(100);
                    }
                    finally
                    {
                        if (connection != null)
                            connection.Disconnect();
                    }
                }
            }
            return Json("");
        }

        public class Setting
        {
            public string Room { get; set; }
            public string Lampa { get; set; }
            public IEnumerable<string> Adresser { get; set; }
            public string StatusAddress { get; internal set; }
        }

        private static void KnxConnected()
        {
            Console.WriteLine("Connected");
        }

        private static void KnxStatusEvent(string address, byte[] state)
        {
            Console.WriteLine("New Status Event: device " + address + " has status " + string.Join(",", state.Select(item => item.ToString())));
        }

        private static void KnxEvent(string address, byte[] state)
        {
            Console.WriteLine("New Event: device " + address + " has status " + string.Join(",", state.Select(item => item.ToString())));
        }
    }
}