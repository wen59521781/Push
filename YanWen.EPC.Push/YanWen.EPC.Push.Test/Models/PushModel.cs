using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanWen.EPC.Push.Test.Models
{
    public class PushModel
    {
        public string BarCode { get; set; }

        public string CreateTime { get; set; }
        public string PushJson { get; set; }
    }
}
