namespace YanWen.EPC.Push.Common.Model
{
    public class PushSqlSentenceModel
    {
        public string SqlId { get; set; }
        public string PushName { get; set; }
        public string SqlContent { get; set; }

        public string SqlContent1 { get; set; }

        /// <summary>
        /// 接口Code码
        /// </summary>
        public string EventCode { get; set; }

        /// <summary>
        /// 推送平台类型
        /// </summary>
        public string TypeId { get; set; }

    }

    public class ResultMsg
    {
        public bool success { get; set; }

        public int code { get; set; }

        public string message { get; set; }

        public object data { get; set; }
    }
}
