using Hemma.Web.Models;
using KNXLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace Hemma.Web.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
    public class BelysningController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View(GetSettings());
        }

        static XmlSerializer serializer = new XmlSerializer(typeof(List<LightSetting>));

        static readonly object settingsFileLock = new object();

        private IEnumerable<LightSetting> GetSettings()
        {
            List<LightSetting> settings = null;
            lock (settingsFileLock)
            {
                var filePath = Server.MapPath("~/Belysning.config");
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    settings = serializer.Deserialize(file) as List<LightSetting>;
                }
            }
            var orderedSettings = settings.OrderBy(item => item.Room).ThenBy(item => item.Lampa);

            return orderedSettings;
        }


        private static readonly object knxLock = new object();

        static ConcurrentStack<int> generatedRandoms = new ConcurrentStack<int>();
        private static readonly object randomLock = new object();

        static int GetNextRandom()
        {
            int number;
            if (generatedRandoms.TryPop(out number))
                return number;
            else
            {
                lock (randomLock)
                {
                    //Check again so no other thread has generated number already
                    if (generatedRandoms.TryPop(out number))
                        return number;

                    var rnd = new Random();
                    for (int i = 0; i < 500; i++)
                    {
                        generatedRandoms.Push(rnd.Next(1500, 5000));
                    }

                    return rnd.Next(1500, 5000);
                }
            }
        }

        [Authorize]
        [HttpGet]
        public void UpdateLightStatus()
        {
            foreach (var key in HttpContext.Application.AllKeys)
                if (key.StartsWith("LightStatus_", StringComparison.InvariantCulture))
                    HttpContext.Application.Remove(key);

            lock (knxLock)
            {
                var localAddress = ConfigurationManager.AppSettings["localAddress"];
                var localPort = GetNextRandom();
                var remoteAddress = ConfigurationManager.AppSettings["remoteAddress"];
                var remotePort = int.Parse(ConfigurationManager.AppSettings["remotePort"]);

                var connection = new KnxConnectionTunneling(remoteAddress, remotePort, localAddress, localPort);
                try
                {
                    connection.KnxStatusDelegate += ReceiveKnxStatus;
                    connection.Connect();
                    Wait(500);
                    var settings = GetSettings();
                    foreach (var setting in settings)
                    {
                        connection.RequestStatus(setting.StatusAddress);
                        Wait(100);
                    }

                    //Wait so statuses can update
                    for (int i = 0; i < 30; i++)
                    {
                        var receivedSettingsCount = HttpContext.Application.AllKeys.Count(item => item.StartsWith("LightStatus_", StringComparison.CurrentCulture));
                        Debug.WriteLine("i: " + i.ToString() + " Received settings count: " + receivedSettingsCount);

                        if (settings.Count() == receivedSettingsCount)
                            break;
                        else
                            Wait(1000);
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("ERROR: " + ex.ToString());
                }
                finally
                {
                    if (connection != null)
                        connection.Disconnect();
                }
            }

        }

        private void Wait(int milliseconds)
        {
            Thread.Sleep(milliseconds);
            //long qwe = 0;
            //var until = DateTime.Now.AddMilliseconds(milliseconds);
            //while (DateTime.Now < until)
            //{
            //    qwe = DateTime.Now.Ticks + 1;
            //}
        }

        private void ReceiveKnxStatus(string address, string state)
        {
            Debug.WriteLine("ReceiveKnxStatus: " + address);
            if (state != null && (int)state[0] > 0)
                HttpContext.Application["LightStatus_" + address] = "on";
            else
                HttpContext.Application["LightStatus_" + address] = "off";
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetLightStatus(string room, string lampa)
        {
            Debug.WriteLine(room + " " + lampa);

            var setting = GetSettings().FirstOrDefault(item => item.RoomNormalized.Equals(room, StringComparison.CurrentCultureIgnoreCase) &&
                item.LampaNormalized.Equals(lampa, StringComparison.CurrentCultureIgnoreCase));

            string state = null;
            if (setting != null)
            {

                var until = DateTime.Now.AddSeconds(30);
                while (DateTime.Now < until)
                {
                    state = HttpContext.Application["LightStatus_" + setting.StatusAddress] as string;
                    if (state != null)
                    {
                        Debug.WriteLine("Got status: " + setting.Room + " " + setting.Lampa);
                        break;
                    }

                    Wait(100);
                }
            }

            if (state == null)
                Debug.WriteLine("Exited without value: " + setting.Room + " " + setting.Lampa);

            return Json(new
            {
                room = room,
                lampa = lampa,
                state = (!string.IsNullOrWhiteSpace(state) ? state : "off")
            }, JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        [HttpPost]
        public JsonResult Ändra(string room, string lampa, string state)
        {
            var setting = GetSettings().FirstOrDefault(item => item.RoomNormalized.Equals(room, StringComparison.CurrentCultureIgnoreCase) &&
                item.LampaNormalized.Equals(lampa, StringComparison.CurrentCultureIgnoreCase));
            if (setting != null)
            {
                lock (knxLock)
                {

                    var localAddress = ConfigurationManager.AppSettings["localAddress"];
                    var localPort = GetNextRandom();
                    var remoteAddress = ConfigurationManager.AppSettings["remoteAddress"];
                    var remotePort = int.Parse(ConfigurationManager.AppSettings["remotePort"]);

                    var connection = new KnxConnectionTunneling(remoteAddress, remotePort, localAddress, localPort);
                    try
                    {
                        connection.KnxConnectedDelegate += KnxConnected;
                        connection.KnxEventDelegate += KnxEvent;
                        connection.KnxStatusDelegate += KnxStatusEvent;
                        connection.Connect();
                        Wait(100);

                        foreach (var address in setting.Addresses)
                        {
                            if (address.OnValue == 1)
                            {
                                var value = (state.ToLower() == "on" ? true : false);
                                //Only allow change if trying to turn off or the light is allowed to be turned on 
                                if (value == false || setting.CanTurnOn)
                                    connection.Action(address.Address, value);
                            }
                            else if (address.OnValue > 1)
                            {
                                var value = (state.ToLower() == "on" ? address.OnValue : 0);
                                //Only allow change if trying to turn off or the light is allowed to be turned on 
                                if (value == 0 || setting.CanTurnOn)
                                    connection.Action(address.Address, value);
                            }

                        }
                        Wait(100);
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


        private static void KnxConnected()
        {
            Console.WriteLine("Connected");
        }

        private static void KnxStatusEvent(string address, string state)
        {
            Console.WriteLine("New Status Event: device " + address + " has status " + state);
        }

        private static void KnxEvent(string address, string state)
        {
            Console.WriteLine("New Event: device " + address + " has status " + state);
        }
    }
}