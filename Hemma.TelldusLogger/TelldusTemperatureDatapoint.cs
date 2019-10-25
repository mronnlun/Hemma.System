using System;
using System.Collections.Generic;
using System.Text;

namespace Hemma.TelldusLogger
{
    public class TelldusTemperatureDatapoint
    {
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public double Temperature { get; internal set; }
        public double Humidity { get; internal set; }
    }
}
