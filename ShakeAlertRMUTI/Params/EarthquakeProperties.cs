using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShakeAlertRMUTI.Params
{
    public class EarthquakeProperties
    {
        public double mag { get; set; }
        public string place { get; set; }
        public long time { get; set; }
        public string url { get; set; }
    }
}
