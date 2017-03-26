using Hemma.Web.Models;
using KNXLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Xml.Serialization;

namespace Hemma.Web.Controllers.Api.v1
{
    public class AlexaBelysningController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Discovery([FromBody] string accesstoken)
        {
            var discoveredAppliances = new List<DiscoveredAppliance>();
            foreach (var setting in GetSettings().OrderBy(item => item.Room).ThenBy(item => item.Lampa))
            {
                discoveredAppliances.Add(new DiscoveredAppliance()
                {
                    applianceId = setting.RoomNormalized + "-" + setting.LampaNormalized,
                    manufacturerName = "Hemma",
                    modelName = "Belysning",
                    version = "v1",
                    friendlyName = setting.AlexaName + " lights",
                    friendlyDescription = setting.Room + " " + setting.Lampa,
                    isReachable = true,
                    actions = new List<string>() { "turnOn", "turnOff" }
                });
            }

            return Ok(discoveredAppliances);
        }

        [HttpPost]
        public IHttpActionResult Action(string function, string applianceId, int state, [FromBody] string accesstoken)
        {
            var roomlamp = applianceId.Split('-');
            var room = roomlamp.ElementAt(0);
            var lampa = roomlamp.ElementAt(1);

            var setting = GetSettings().FirstOrDefault(item => item.RoomNormalized.Equals(room, StringComparison.CurrentCultureIgnoreCase) &&
               item.LampaNormalized.Equals(lampa, StringComparison.CurrentCultureIgnoreCase));
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
                        //connection.KnxConnectedDelegate += KnxConnected;
                        //connection.KnxEventDelegate += KnxEvent;
                        //connection.KnxStatusDelegate += KnxStatusEvent;
                        connection.Connect();
                        Thread.Sleep(100);

                        foreach (var address in setting.Addresses)
                        {
                            if (address.OnValue == 1)
                            {
                                var value = (state == 1 ? true : false);
                                //Only allow change if trying to turn off or the light is allowed to be turned on 
                                if (value == false || setting.CanTurnOn)
                                    connection.Action(address.Address, value);
                            }
                            else if (address.OnValue > 1)
                            {
                                var value = (state == 1 ? address.OnValue : 0);
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

            return Ok();
        }

        static XmlSerializer serializer = new XmlSerializer(typeof(List<LightSetting>));

        static readonly object settingsFileLock = new object();

        private IEnumerable<LightSetting> GetSettings()
        {
            List<LightSetting> settings = null;
            lock (settingsFileLock)
            {
                var filePath = HttpContext.Current.Server.MapPath("~/Belysning.config");
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    settings = serializer.Deserialize(file) as List<LightSetting>;
                }
            }
            var orderedSettings = settings.OrderBy(item => item.Room).ThenBy(item => item.Lampa);

            return orderedSettings;
        }

        public class DiscoveredAppliance
        {
            public string applianceId { get; set; }
            public string manufacturerName { get; set; }
            public string modelName { get; set; }
            public string version { get; set; }
            public string friendlyName { get; set; }
            public string friendlyDescription { get; set; }
            public bool isReachable { get; set; }
            public IList<string> actions { get; set; }
        }
    }
}
