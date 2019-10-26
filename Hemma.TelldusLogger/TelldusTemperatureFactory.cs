using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Hemma.TelldusLogger
{
    public interface ITelldusTemperatureFactory
    {
        TelldusTemperatureDatapoint Create(string input);
    }

    public class TelldusTemperatureFactory : ITelldusTemperatureFactory
    {
        public TelldusTemperatureDatapoint Create(string input)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            var message = JsonSerializer.Deserialize<TelldusMessageRaw>(input, options);

            var temperature = message.Data.First(item => item.Name.Equals("temp", StringComparison.OrdinalIgnoreCase));
            var humidity = message.Data.First(item => item.Name.Equals("humidity", StringComparison.OrdinalIgnoreCase));

            var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            var localTimestamp = epochStart.AddSeconds(temperature.LastUpdated).ToLocalTime();

            var datapoint = new TelldusTemperatureDatapoint();
            datapoint.Name = message.Name;
            datapoint.Battery = int.Parse(message.Battery);

            datapoint.Timestamp = localTimestamp.Ticks;
            datapoint.Datestamp = localTimestamp;

            datapoint.Temperature = double.Parse(temperature.Value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture);
            datapoint.Humidity = double.Parse(humidity.Value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture);

            return datapoint;
        }

        class TelldusMessageRaw
        {
            public List<TelldusDatapointRaw> Data { get; set; }

            public string Name { get; set; }
            public string Battery { get; set; }
        }

        class TelldusDatapointRaw
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public long LastUpdated { get; set; }
        }

    }
}