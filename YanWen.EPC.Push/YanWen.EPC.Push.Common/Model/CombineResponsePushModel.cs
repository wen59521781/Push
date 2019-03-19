using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanWen.EPC.Push.Common.Model
{
    public class CombineResponsePushModel
    {
        public string Id { get; set; }
        public string Number { get; set; }
        /// <summary>
        /// 请求参数
        /// </summary>
        public string RequestJson { get; set; }

        /// <summary>
        /// 返回参数
        /// </summary>
        public string ResponseJson { get; set; }
        public string StatusId { get; set; }
        public string InsertDate { get; set; }
        public string PushNum { get; set; }

        public string Parameter { get; set; }

        public string Parameter1 { get; set; }

        public string MainPart { get; set; }

        public string Action { get; set; }

        public string DbAction { get; set; }

      
    }
}
