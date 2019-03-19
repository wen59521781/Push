using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanWen.EPC.Push.Common.Model
{
    public class PushMessageModel
    {

        public string Type { get; set; }

        public string TaskId { get; set; }

        public string Number { get; set; }

        public string Msg { get; set; }

        public string Table { get; set; }

        public string Rjson { get; set; }

        public string Fjson { get; set; }

        public string Node { get; set; }
    }
}
