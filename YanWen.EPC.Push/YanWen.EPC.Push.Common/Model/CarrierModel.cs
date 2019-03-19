using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanWen.EPC.Push.Common.Model
{
    public class CarrierModel
    {
        public string CarrierId { get; set; }

        public string OnlineProductCode { get; set; }

        public string YanWenProductCode { get; set; }

        public string ServiceCode { get; set; }

        public string SupplierCode { get; set; }

        public string RuleCode { get; set; }

        public int MinWeight { get; set; }

        public int MaxWeight { get; set; }

    }
}
