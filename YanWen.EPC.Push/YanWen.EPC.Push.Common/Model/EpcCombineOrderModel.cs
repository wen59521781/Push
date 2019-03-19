using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanWen.EPC.Push.Common.Model
{
    public class EpcCombineOrderModel
    {
        public string api_key { get; set; }
        public string[] orders { get; set; }
        public float total_weight { get; set; }
        public decimal total_value { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public bool has_liquid { get; set; }
        public bool has_battery { get; set; }
        public bool has_sensitive { get; set; }
        public string operator_id { get; set; }

    }
}
