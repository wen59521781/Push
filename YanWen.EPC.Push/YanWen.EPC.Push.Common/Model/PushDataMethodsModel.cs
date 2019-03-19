using System;

namespace YanWen.EPC.Push.Common.Model
{
    public class PushDataMethodsModel
    {
        public string Id { get; set; }
        public string SqlId { get; set; }
        public string Number1 { get; set; }
        public string Number2 { get; set; }
        public int StatusId { get; set; }
        public DateTime InsertDate { get; set; }
        public object timestamp { get; set; }
        public int PushNum { get; set; }

        public string TypeId { get; set; }

        public string Parameter { get; set; }


        public string MainPart { get; set; }

        public string Action { get; set; }

        public string DbAction { get; set; }


    }
}
