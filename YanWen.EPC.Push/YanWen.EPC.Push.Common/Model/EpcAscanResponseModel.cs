using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanWen.EPC.Push.Common.Model
{
    public class EpcAscanResponseModel
    {

        public string timestamp { get; set; }
        public int code { get; set; }
        public Datum[] data { get; set; }

        public class Datum
        {
            public int action { get; set; }
            public string message { get; set; }
            public string tracking_id { get; set; }
        }

    }
}
