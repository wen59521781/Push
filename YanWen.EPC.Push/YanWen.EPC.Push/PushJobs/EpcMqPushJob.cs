using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComputerCenterScheduling;
using log4net;
using YanWen.EPC.Push.Common.DataAccess;
using Yanwen.EPC.Common.DataAccess;
using Yanwen.EPC.Common.Logic;
using YanWen.EPC.Push.Common.Model;

namespace YanWen.EPC.Push.PushJobs
{
    public class EpcMqPushJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger("[EPC-MQ推送]");
        private static readonly string DelayMinutes = ConfigurationManager.AppSettings["delayMinutes"];
        private static readonly int RecordsNumber = int.Parse(ConfigurationManager.AppSettings["RecordsNumber"]);
        private static readonly int FailPushNumber = int.Parse(ConfigurationManager.AppSettings["FailPushNumber"]);


        public async Task RunAsync(IJobContext context, string sqlId, string statusId)
        {
            var list = PushDataDal.Instance.GetPushList(RecordsNumber, FailPushNumber, 1);
            if (list.Any())
            {
                Parallel.ForEach(list, t =>
                {
                    switch (t.MainPart)
                    {
                        case "EPC_CombinedPackage"://合包推送
                            try
                            {
                                //MQ推送
                                var mqDataModel = EpcMqPushTakDal.Instance.GetCombinedPackageByBarCode(t.Number);
                                var mqpushModel = MqPushLogic.Instance.ConvertToMqPushModel(t.MainPart, t.Action, t.Number, mqDataModel, t.DbAction);
                                //向MQPushTask表插入数据
                                EpcMqPushTakDal.Instance.InsertPush(mqpushModel.Data);
                                var badState = PushDataDal.Instance.UpState(new PushDataMethodsModel()
                                {
                                    Number1 = t.Number,
                                    Id = t.Id,
                                }, 2, "[dbo].[EPC_MqPushTask]");
                                if (badState.HasError)
                                {
                                    Logger.Error($"单号[{t.Number}]修改推送状态时出错" + badState.ErrorMsg);
                                }
                                else
                                {
                                    Logger.Error($"单号[{t.Number}]推送成功！");
                                }

                            }
                            catch (Exception e)
                            {
                                Logger.Error($"单号[{t.Number}]推送MQ出错" + e.Message);
                                var badState = PushDataDal.Instance.UpState(new PushDataMethodsModel()
                                {
                                    Number1 = t.Number,
                                    Id = t.Id,
                                }, 3, "[dbo].[EPC_MqPushTask]");
                                if (badState.HasError)
                                {
                                    Logger.Error($"单号[{t.Number}]修改推送状态时出错" + badState.ErrorMsg);
                                }
                            }
                            break;
                    }
                });
            }
            Thread.Sleep(int.Parse(DelayMinutes) * 1000);
        }
    }
}
