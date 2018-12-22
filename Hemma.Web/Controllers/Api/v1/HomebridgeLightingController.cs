using Hemma.Web.Models;
using KNXLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Xml.Serialization;

namespace Hemma.Web.Controllers.Api.v1
{
    public class HomebridgeLightingController : ApiController
    {
        [AllowAnonymous]
        [HttpGet, HttpPost]
        public IHttpActionResult SwitchOn(string light)
        {
            ChangeState(light, true);

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet, HttpPost]
        public IHttpActionResult SwitchOff(string light)
        {
            ChangeState(light, false);
            return Ok();
        }

        public void ChangeState(string light, bool turnOn)
        {
            var setting = GetSettings().FirstOrDefault(item => item.AlexaName.Equals(light, StringComparison.CurrentCultureIgnoreCase));

            if (setting != null)
            {
                lock (BelysningController.knxLock)
                {
                    var localAddress = ConfigurationManager.AppSettings["localAddress"];
                    var localPort = BelysningController.GetNextRandom();
                    var remoteAddress = ConfigurationManager.AppSettings["remoteAddress"];
                    var remotePort = int.Parse(ConfigurationManager.AppSettings["remotePort"]);

                    var connection = new KnxConnectionTunneling(remoteAddress, remotePort, localAddress, localPort);
                    try
                    {
                        connection.Connect();
                        Thread.Sleep(100);

                        foreach (var address in setting.Addresses)
                        {
                            if (address.OnValue == 1)
                            {
                                //Only allow change if trying to turn off or the light is allowed to be turned on 
                                if (turnOn == false || setting.CanTurnOn)
                                    connection.Action(address.Address, turnOn);
                            }
                            else if (address.OnValue > 1)
                            {
                                var value = (turnOn ? address.OnValue : 0);
                                //Only allow change if trying to turn off or the light is allowed to be turned on 
                                if (value == 0 || setting.CanTurnOn)
                                    connection.Action(address.Address, value);
                            }

                        }
                        Thread.Sleep(100);
                    }
                    finally
                    {
                        if (connection != null)
                            connection.Disconnect();
                    }
                }
            }
        }

        public static readonly object knxLock = new object();
        static ConcurrentStack<int> generatedRandoms = new ConcurrentStack<int>();
        private static readonly object randomLock = new object();

        public static int GetNextRandom()
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

        private void ReceiveKnxStatus(string address, string state)
        {
            Debug.WriteLine("ReceiveKnxStatus: " + address);
            if (state != null && (int)state[0] > 0)
                _lightStatusCache["LightStatus_" + address] = "on";
            else
                _lightStatusCache["LightStatus_" + address] = "off";
        }

        static readonly object settingsFileLock = new object();
        static XmlSerializer serializer = new XmlSerializer(typeof(List<LightSetting>));

        static ConcurrentDictionary<string, string> _lightStatusCache = new ConcurrentDictionary<string, string>();

        private IEnumerable<LightSetting> GetSettings()
        {
            List<LightSetting> settings = null;
            lock (settingsFileLock)
            {
                var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Belysning.config");
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    settings = serializer.Deserialize(file) as List<LightSetting>;
                }
            }
            var orderedSettings = settings.OrderBy(item => item.Room).ThenBy(item => item.Lampa);

            return orderedSettings;
        }

        [AllowAnonymous]
        [HttpGet, HttpPost]
        public IHttpActionResult SwitchStatus(string light)
        {
            LightSetting lightSetting = null;
            var settings = GetSettings();
            foreach (var setting in settings)
            {
                if (setting.AlexaName.Equals(light, StringComparison.InvariantCultureIgnoreCase))
                {
                    lightSetting = setting;
                    break;
                }
            }

            if (lightSetting == null)
                return NotFound();

            foreach (var key in _lightStatusCache.Keys)
                if (key.StartsWith("LightStatus_" + lightSetting.StatusAddress, StringComparison.InvariantCulture))
                {
                    string value;
                    _lightStatusCache.TryRemove(key, out value);
                }

            lock (knxLock)
            {
                var localAddress = ConfigurationManager.AppSettings["localAddress"];
                var localPort = GetNextRandom();
                var remoteAddress = ConfigurationManager.AppSettings["remoteAddress"];
                var remotePort = int.Parse(ConfigurationManager.AppSettings["remotePort"]);

                var connection = new KnxConnectionTunneling(remoteAddress, remotePort, localAddress, localPort);
                try
                {
                    connection.KnxStatusDelegate = (string address, string state) =>
                        {
                            Debug.WriteLine("ReceiveKnxStatus: " + address);
                            if (state != null && (int)state[0] > 0)
                                _lightStatusCache["LightStatus_" + address] = "on";
                            else
                                _lightStatusCache["LightStatus_" + address] = "off";
                        };

                    connection.Connect();
                    Thread.Sleep(500);

                    connection.RequestStatus(lightSetting.StatusAddress);

                    //Wait so statuses can update
                    for (int i = 0; i < 100; i++)
                    {
                        var statusCache = _lightStatusCache.Keys.FirstOrDefault(item => item.StartsWith("LightStatus_" + lightSetting.StatusAddress, StringComparison.CurrentCulture));

                        if (statusCache != null)
                            break;
                        else
                            Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ERROR: " + ex.ToString());
                }
                finally
                {
                    if (connection != null)
                        connection.Disconnect();
                }
            }

            string lightingStatus;
            _lightStatusCache.TryGetValue("LightStatus_" + lightSetting.StatusAddress, out lightingStatus);

            if (lightingStatus != null && lightingStatus.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                return Ok(1);
            else
                return Ok(0);
        }

    }
}