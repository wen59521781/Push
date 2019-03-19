using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanWen.EPC.Push.Common.Model
{
    public class DingDingMsgModel
    {
        public string msgtype { get; set; }
        public TextModel text { get; set; }
        public At at { get; set; }
    }


    public class MarkdownModel
    {
        public string msgtype { get; set; }
        public Markdown markdown { get; set; }
        public At at { get; set; }
    }

    public class Markdown
    {
        public string title { get; set; }
        public string text { get; set; }
    }

    public class At
    {
        public string[] atMobiles { get; set; }
        public bool isAtAll { get; set; }
    }


    public class TextModel
    {
        public string content { get; set; }
    }



}
