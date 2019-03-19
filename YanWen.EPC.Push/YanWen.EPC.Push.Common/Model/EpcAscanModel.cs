using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanWen.EPC.Push.Common.Model
{
    public class EpcAscanModel
    {
        public string api_key { get; set; }
        public List<TrackingModel> data { get; set; }


        public class TrackingModel
        {
            public string tracking_id { get; set; }
            public float weight { get; set; }
            public string has_liquid { get; set; }
            public string has_battery { get; set; }
            public string has_sensitive { get; set; }
            public string in_date { get; set; }
        }

    }
}
