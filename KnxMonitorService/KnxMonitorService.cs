using KNXLib;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KnxMonitorService
{
    public partial class KnxMonitorService : ServiceBase
    {
        public KnxMonitorService()
        {
            InitializeComponent();
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected override void OnStart(string[] args)
        {
            logger.Info("KnMonitorService started");

            var localAddress = "192.168.0.102";
            var localPort = 1450;
            var remoteAddress = "192.168.0.105";
            var remotePort = 3671;

            _connection = new KnxConnectionTunneling(remoteAddress, remotePort, localAddress, localPort);
            _connection.KnxEventDelegate += KnxGeneralEvent;
            _connection.KnxStatusDelegate += KnxStatusEvent;
            _connection.Connect();
            Thread.Sleep(500);

        }

        KnxConnectionTunneling _connection = null;

        private readonly object knxLock = new object();

        void KnxAction(string address, bool stateOn)
        {
            lock (knxLock)
            {
                _connection.Action(address, stateOn);
                Thread.Sleep(100);
                logger.Info("Action {0} address {1}", stateOn, address);
            }
        }
        void RequestKnxStatus(string address)
        {
            lock (knxLock)
            {
                _connection.RequestStatus(address);
                Thread.Sleep(100);
                logger.Info("RequestKnxStatus address {0}", address);
            }
        }

        private void KnxGeneralEvent(string address, string state)
        {
            //Console.WriteLine("KnxGeneralEvent: " + address + " State: " + (int)state[0]);

            logger.Info("KnxGeneralEvent address {0} state {1}", address, state);

            if (address == "2/0/1")
            {
                var arbetsrumBordslampaId = 1487762;
                var vardagsrumSkänkLampaId = 1487766;
                var laminoLampaId = 3531075;

                var action = "On";
                if (((int)state[0]) == 0)
                    action = "Off";

                ChangeLight(arbetsrumBordslampaId, action);
                ChangeLight(vardagsrumSkänkLampaId, action);
                ChangeLight(laminoLampaId, action);
            }
        }

        private void ChangeLight(int lightId, string action)
        {
            const string consumerKey = "FEHUVEW84RAFR5SP22RABURUPHAFRUNU";
            const string consumerSecret = "ZUXEVEGA9USTAZEWRETHAQUBUR69U6EF";
            const string token = "f212fc0afc552a2a384ff98cff7283b105866a56a";
            const string tokenSecret = "cb6ae27cbcbcc79d5d6809c367ec3b4a";

            var now = Math.Floor(DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
            string nonce = DateTime.Now.Ticks.ToString();
            string authoriationHeader = "oauth_timestamp=\"" + now + "\", oauth_version=\"1.0\", oauth_signature_method=\"PLAINTEXT\", oauth_consumer_key=\"" + consumerKey
                + "\", oauth_token=\"" + token + "\", oauth_signature=\"" + consumerSecret + "%26" + tokenSecret + "\", oauth_nonce=\"" + nonce + "\"";

            var url = "http://api.telldus.com/json/device/turn" + action + "?id=" + lightId;
            var changeLightRequest = HttpWebRequest.CreateHttp(url);
            changeLightRequest.Timeout = 5000;
            changeLightRequest.Headers.Add(HttpRequestHeader.Authorization, "OAuth " + authoriationHeader);

            //Console.WriteLine("Executing url: " + url);
            logger.Info("Executing url {0}", url);

            try
            {
                var changeLightResponse = changeLightRequest.GetResponse();
                changeLightResponse.Close();
            }
            catch (WebException ex)
            {
                logger.Error(ex, "ChangeLight {0} action {1}", lightId, action);
                //Console.WriteLine(ex.Message);
            }
            logger.Info("Executed url {0}", url);
            //Console.WriteLine("Executed url: " + url);
        }

        private void KnxStatusEvent(string address, string state)
        {
            logger.Info("KnxStatusEvent address {0} state {1}", address, state);
            //Console.WriteLine("KnxStatusEvent: " + address + " State: " + (int)state[0]);
        }

        protected override void OnStop()
        {
            logger.Info("KnxMonitorService stopped");
            if (_connection != null)
                _connection.Disconnect();
        }
    }
}
