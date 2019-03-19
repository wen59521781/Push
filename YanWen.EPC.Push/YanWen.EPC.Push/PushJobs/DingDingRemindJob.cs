using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComputerCenterScheduling;
using log4net.Repository.Hierarchy;
using Yanwen.EPC.Common.Logic;
using Yanwen.Framework.Core.Extensions;
using YanWen.EPC.Push.Common.Api;
using YanWen.EPC.Push.Common.DataAccess;
using YanWen.EPC.Push.Common.Model;

namespace YanWen.EPC.Push.PushJobs
{
    public class DingDingRemindJob : IJob
    {

        public async Task RunAsync(IJobContext context, string sqlId, string statusId)
        {
            var hour = DateTime.Now.Hour;
            if (hour == 10 || hour == 16)
            {
                var list = PushDataDal.Instance.GetBadPushList();
                var str = $"[EPC A-Scan+合包]系统检测有[{list.Count}]单推送给WISH失败了！相关错误单号的查询语句如下：" +
                          $"\r\n select  * from EPC_PushMessage where  Taskid in (";
                if (list.Any())
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (i != (list.Count - 1))
                        {
                            str += $"'{list[i]}',";
                        }
                        else
                        {
                            str += $"'{list[i]}'";
                        }
                    }
                    DingDing(str + ")");
                }
            }
            Thread.Sleep(60 * 1000 * 60);
        }

        public void DingDing(string msg)
        {
            var url = "https://oapi.dingtalk.com/robot/send?access_token=5f534e705e8965260dbae62e29c9abc7b5bdb9ff5c43af1914426ac2d8516d81";
            var mo = new DingDingMsgModel()
            {
                msgtype = "text",
                text = new TextModel()
                {
                    content = msg,
                },
                at = new At()
                {
                    atMobiles = new[] { "18731192118" },
                    isAtAll = false
                },
            };
            string response = CommonApi.Instance.HttpPostConnectToServer(url, mo.ToJsonString());
        }
    }
}
