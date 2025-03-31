using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShakeAlertRMUTI.Params
{
    public class MsTeamsCallData
    {
        public string webHookId {  get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string[] button { get; set; }
    }
}
