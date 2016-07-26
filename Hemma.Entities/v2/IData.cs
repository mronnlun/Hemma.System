using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hemma.Entities.v2
{
    public interface IData
    {
        DateTime Datestamp { get; set; }
        long Timestamp { get; set; }
    }
}
