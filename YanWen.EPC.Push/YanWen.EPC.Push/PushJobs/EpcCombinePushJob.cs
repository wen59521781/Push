using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComputerCenterScheduling;
using log4net;
using Newtonsoft.Json;
using Yanwen.Framework.Core.Extensions;
using Yanwen.TMS.Base;
using Yanwen.TMS.Common.Cache;
using Yanwen.TMS.Common.Model;
using YanWen.EPC.Push.Common.DataAccess;
using YanWen.EPC.Push.Common.Model;
using Yanwen.TMS.Common.Logic;
using YanWen.EPC.Push.Common.Logic;

namespace YanWen.EPC.Push.PushJobs
{
    public class EpcCombinePushJob : IJob
    {
        private static readonly string DelayMinutes = ConfigurationManager.AppSettings["delayMinutes"];
        private static readonly ILog Logger = LogManager.GetLogger("[EPC合包-TMS录入]");
        private static readonly int RecordsNumber = int.Parse(ConfigurationManager.AppSettings["RecordsNumber"]);
        private static readonly int FailPushNumber = int.Parse(ConfigurationManager.AppSettings["FailPushNumber"]);
        private static readonly string ApiUrl = ConfigurationManager.AppSettings["PushUrl"];
        private static readonly string ApiKey = ConfigurationManager.AppSettings["ApiKey"];
        private static string WishExpressSettingUri = ConfigurationManager.AppSettings["WishExpressSettingUri"];

        public async Task RunAsync(IJobContext context, string sqlId, string statusId)
        {
            var list = PushDataDal.Instance.GetPushList(RecordsNumber, FailPushNumber, 0);
            if (list.Any())
            {
                foreach (var t in list)
                {
                    if (!string.IsNullOrWhiteSpace(t.ResponseJson))
                    {
                        var model = JsonConvert.DeserializeObject<EpcCombineResponseModel>(t.ResponseJson);
                        var requestModel = JsonConvert.DeserializeObject<EpcCombineOrderModel>(t.RequestJson);
                        switch (model.carrier)
                        {
                            //燕文的货物调接口
                            case 68:
                            case 69:
                            case 60:
                            case 61:
                                #region 走wish录入

                                var apiRequestDto = new ApiRequestModel
                                {
                                    WaybillNumber = model.barcode,
                                    FromName = "EPC-Wish录入",
                                    CreateId = "1001",
                                    CompanyCode = "10",
                                    Weight = EpcDal.Instance.GetEpcCombineWeight(requestModel.orders.ToList(), t.Parameter,t.Parameter1)
                                };
                                var uri = new Uri($"{WishExpressSettingUri}/expressDetail");
                                var result = uri.PostData<Result<WishExpressModel>>(apiRequestDto.ToJsonString());
                                //todo 替换产品
                                if (result.HasError)
                                {
                                    ErrorLog(t, 0, "", $"合包ID{t.Number}合并运单号[{model.barcode}]调取录入数据出错详情：{result.ErrorMsg}");
                                }
                                else
                                {
                                    var resultadd = SaveExpressTemLogic.Instance.AddExpress(result.Data, model.carrier);
                                    if (resultadd.HasError)
                                    {
                                        ErrorLog(t, 0, "", $"单号[{t.Number}]保存快件时出错!{resultadd.ErrorMsg}");
                                    }
                                    else//保存成功
                                    {
                                        ErrorLog(t, 1, "", "");
                                        var mod = SaveExpressTemLogic.Instance.ConversionModel(model, requestModel.orders.ToList(), t, result.Data.Express.ExchangeNumber);
                                        var ress = EpcDal.Instance.SaveEpcCombine(mod, requestModel.orders.ToList());
                                        if (ress.HasError)
                                        {
                                            ErrorLog(t, 0, "", $"单号[{t.Number}]保存epc合包表时出错!{ress.ErrorMsg}");
                                        }
                                        else//保存成功
                                        {
                                            ErrorLog(t, 1, "", "");
                                        }
                                    }
                                }

                                #endregion
                                break;
                            default:

                                #region 直接保存EPC
                                var mo = SaveExpressTemLogic.Instance.ConversionModel(model, requestModel.orders.ToList(), t, "");
                                var re = EpcDal.Instance.SaveExpress(mo, requestModel.orders.ToList());
                                if (re.HasError)
                                {
                                    ErrorLog(t, 0, "", $"单号[{t.Number}]保存快件临时表时出错!{re.ErrorMsg}");
                                }
                                else
                                {
                                    var res = EpcDal.Instance.SaveEpcCombine(mo, requestModel.orders.ToList());
                                    if (res.HasError)
                                    {
                                        ErrorLog(t, 0, "", $"单号[{t.Number}]保存epc合包表时出错!{res.ErrorMsg}");
                                    }
                                    else
                                    {
                                        ErrorLog(t, 1, "", "");
                                    }
                                }

                                #endregion
                                break;
                        }
                    }
                    else
                    {
                        Logger.Error($"单号[{t.Number}]合包json数据为空！");
                    }
                }
            }
            Thread.Sleep(int.Parse(DelayMinutes) * 1000);
        }




        /// <summary>
        /// 修改推送状态是否成功 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="type">0 修改失败 1修改成功</param>
        /// <param name="json">json数据</param>
        /// <param name="msg">首要错误信息</param>
        public void ErrorLog(CombineResponsePushModel t, int type, string json, string msg)
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                Logger.Error(msg);
                PushDataDal.Instance.SavePushMessage(new PushMessageModel { Type = "0", TaskId = t.Id, Number = t.Number, Msg = msg, Table = "EPC_CombineResponse", Rjson = json, Fjson = "", Node = "TMS-Input" });
            }
            var badState = PushDataDal.Instance.UpState(t, type);
            if (badState.HasError)
            {
                Logger.Error($"单号[{t.Number}]修改推送状态时出错" + badState.ErrorMsg);
                return;
            }
            if (type == 1)
            {
                Logger.Info($"单号[{t.Number}]推送成功");
                PushDataDal.Instance.SavePushMessage(new PushMessageModel { Type = "0", TaskId = t.Id, Number = t.Number, Msg = msg, Table = "EPC_CombineResponse", Rjson = json, Fjson = "", Node = "TMS-Input" });
            }
        }

    }
}
