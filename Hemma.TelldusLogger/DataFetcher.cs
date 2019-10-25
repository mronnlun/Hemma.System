using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hemma.TelldusLogger
{
    public interface IDataFetcher
    {
        Task<string> GetDatapoint(string sensorid);
    }
    public class DataFetcher : IDataFetcher
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;

        public DataFetcher(IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetDatapoint(string sensorid)
        {
            string consumerKey = this.configuration["TelldusConsumerKey"];
            string consumerSecret = this.configuration["TelldusConsumerSecret"];
            string token = this.configuration["TelldusToken"];
            string tokenSecret = this.configuration["TelldusTokenSecret"];

            var now = Math.Floor(DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
            string nonce = DateTime.Now.Ticks.ToString();
            string authoriationHeader = "oauth_timestamp=\"" + now + "\", oauth_version=\"1.0\", oauth_signature_method=\"PLAINTEXT\", oauth_consumer_key=\"" + consumerKey
                + "\", oauth_token=\"" + token + "\", oauth_signature=\"" + consumerSecret + "%26" + tokenSecret + "\", oauth_nonce=\"" + nonce + "\"";

            var url = "http://api.telldus.com/json/sensor/info" + "?id=" + sensorid;

            var httpClient = this.httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authoriationHeader);
            var json = await httpClient.GetStringAsync(url);

            return json;

        }

    }
}