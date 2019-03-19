using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanWen.EPC.Push.Common.Model
{
    public class EpcCombineModel
    {
        public string BarCode { get; set; }
        public string ExchangeNumber { get; set; }
        public string CombineId { get; set; }
        public string pdf_10_lcl_url { get; set; }
        public string pdf_a4_en_url { get; set; }
        public string pdf_a4_lcl_url { get; set; }
        public string pdf_10_en_url { get; set; }
        public string label_format { get; set; }
        public int carrier { get; set; }
        public int Weight { get; set; }
        public int Piece { get; set; }
        public string PackageLabelsType { get; set; }
        public string CreateTime { get; set; }
    }


}
