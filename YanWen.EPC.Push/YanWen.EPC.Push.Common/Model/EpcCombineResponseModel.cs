using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanWen.EPC.Push.Common.Model
{
    public class EpcCombineResponseModel
    {
        public string status { get; set; }
        public string pdf_10_lcl_url { get; set; }
        public string pdf_a4_en_url { get; set; }
        public string pdf_a4_lcl_url { get; set; }
        public string pdf_10_en_url { get; set; }
        public string timestamp { get; set; }
        public string barcode { get; set; }
        public string label_format { get; set; }
        /// <summary>
        /// 以此区分是否走燕文录入
        /// </summary>
        public int carrier { get; set; }     
        public string message { get; set; }
        public int error_code { get; set; }

    }

    public class EpcCombineErrorResponseModel
    {
            public string status { get; set; }
            public string timestamp { get; set; }
            public string message { get; set; }
            public int error_code { get; set; }      
    }
}
