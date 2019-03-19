using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanWen.EPC.Push.Common.Model
{
    public class EpcSendRequestModel
    {
        public string api_key { get; set; }
        public string carrier_code { get; set; }
        public Tracking[] trackings { get; set; }

        public class Tracking
        {
            public string tracking_number { get; set; }
            public string current_tracking_status { get; set; }
            public string destination_country { get; set; }
            public string origin_country { get; set; }
            public Checkpoint[] checkpoints { get; set; }
        }

        public class Checkpoint
        {
            public string event_status { get; set; }
            public float weight { get; set; }
            public string location { get; set; }
            public string tracking_status { get; set; }
            public string timezone { get; set; }
            public string country_code { get; set; }
            public string package_no { get; set; }
            public object city { get; set; }
            public object state { get; set; }
            public object postal_code { get; set; }
            public string timestamp { get; set; }
            public string message { get; set; }
        }


        public class EpcSendResultModel
        {
            public int code { get; set; }
            public string timestamp { get; set; }
            public string msg { get; set; }
            public TrackingModel[] trackings { get; set; }
        }

        public class TrackingModel
        {
            public string msg { get; set; }
            public string tracking_number { get; set; }
            public int code { get; set; }
            public string warning { get; set; }
        }

    }
}
