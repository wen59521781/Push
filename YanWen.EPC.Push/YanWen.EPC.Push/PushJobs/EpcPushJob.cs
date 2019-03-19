using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComputerCenterScheduling;
using log4net;
using Newtonsoft.Json;
using Yanwen.Framework.Core.Extensions;
using YanWen.EPC.Push.Common.Api;
using YanWen.EPC.Push.Common.DataAccess;
using YanWen.EPC.Push.Common.Model;

namespace YanWen.EPC.Push.PushJobs
{
    public class EpcPushJob : IJob
    {
        private static readonly string DelayMinutes = ConfigurationManager.AppSettings["delayMinutes"];
        private static readonly ILog Logger = LogManager.GetLogger("[A-Scan+合包]");
        private static readonly int RecordsNumber = int.Parse(ConfigurationManager.AppSettings["RecordsNumber"]);
        private static readonly int FailPushNumber = int.Parse(ConfigurationManager.AppSettings["FailPushNumber"]);
        private static readonly string ApiUrl = ConfigurationManager.AppSettings["PushUrl"];
        private static readonly string ApiKey = ConfigurationManager.AppSettings["ApiKey"];


        /// <summary>
        /// EPC数据推送
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sqlId"></param>
        /// <param name="statusId"></param>
        /// <returns></returns>
        public async Task RunAsync(IJobContext context, string sqlId, string statusId)
        {
            try
            {
                var pushsql = PushDataDal.Instance.GetAllPushSql().Where(t => t.TypeId == "101");
                foreach (var item in pushsql)
                {
                    var list = PushDataDal.Instance.GetPushList(RecordsNumber, FailPushNumber, item.SqlId);
                    if (list.Any())
                    {
                        #region 并行循环                                              
                        //Parallel.ForEach(list, t =>
                        //{
                        //    var json = "";
                        //    switch (t.SqlId)
                        //    {
                        //        case "1":

                        //            #region 小件录入节点推送
                        //            {
                        //                var data = PushDataDal.Instance.GetPushData<EpcAscanModel.TrackingModel>(item.SqlContent, t.Number1).FirstOrDefault();
                        //                if (data == null)
                        //                {
                        //                    json = "";
                        //                }
                        //                else
                        //                {
                        //                    data.in_date = data.in_date.Replace("/", "-");
                        //                    var mo = new EpcAscanModel()
                        //                    {
                        //                        api_key = ApiKey,
                        //                        data = new List<EpcAscanModel.TrackingModel>() { data },
                        //                    };
                        //                    json = mo.ToJsonString();
                        //                }
                        //                if (string.IsNullOrWhiteSpace(json))
                        //                {
                        //                    ErrorLog(t, 0, "", $"没有获取到Json数据");
                        //                }
                        //                else
                        //                {

                        //                    var urie = ApiUrl + $"/{item.EventCode}";
                        //                    var result = PostApi<EpcAscanResponseModel>(t, urie, json);
                        //                    //var result = urie.PostData<EpcAscanResponseModel>(json);
                        //                    var rjson = result.ToJsonString();
                        //                    if (result != null)
                        //                    {
                        //                        if (result.data[0].message != "Success")
                        //                        {
                        //                            ErrorLog(t, 0, rjson, $"调取WISH接口出错！\r\n 请求JSON：{json}\r\n 返回JSON：" + rjson);
                        //                        }
                        //                        else //成功修改状态
                        //                        {
                        //                            ErrorLog(t, 1, rjson, "");
                        //                        }
                        //                    }
                        //                }
                        //                break;
                        //            }
                        //        #endregion

                        //        case "2":
                        //        case "3"://立即发货

                        //            #region 合包
                        //            {
                        //                //合包明细
                        //                List<string> orders;
                        //                if (t.SqlId == "3")
                        //                {
                        //                    orders = new List<string>()
                        //                    {
                        //                        JsonConvert.DeserializeObject<string>(t.Number2)
                        //                    };
                        //                }
                        //                else
                        //                {
                        //                    orders = JsonConvert.DeserializeObject<List<string>>(t.Number2);
                        //                }


                        //                var data = EpcDal.Instance.GetCombineOrderModel(orders);
                        //                if (orders.Any())
                        //                {
                        //                    data.api_key = ApiKey;
                        //                    data.orders = orders.ToArray();
                        //                    data.total_weight = EpcDal.Instance.GetEpcCombineWeight(orders, t.Parameter);
                        //                    json = data.ToJsonString();
                        //                }
                        //                else
                        //                {
                        //                    json = "";
                        //                }
                        //                if (string.IsNullOrWhiteSpace(json))
                        //                {
                        //                    ErrorLog(t, 0, "", $"没有获取到Json数据");
                        //                }
                        //                else
                        //                {
                        //                    var urie = ApiUrl + $"/{item.EventCode}";
                        //                    var result = PostApi<EpcCombineResponseModel>(t, urie, json);
                        //                    //var result = urie.PostData<EpcCombineResponseModel>(json);
                        //                    var rjson = result.ToJsonString();
                        //                    if (result != null)
                        //                    {
                        //                        if (result.status != "Success")
                        //                        {
                        //                            ErrorLog(t, 0, rjson, $"调取WISH接口出错！请求JSON：{json}\r\n返回JSON：" + rjson);
                        //                        }
                        //                        else //成功修改状态
                        //                        {
                        //                            var res = PushDataDal.Instance.SaveCombineResponse(t, json, rjson);
                        //                            if (res.HasError)
                        //                            {
                        //                                Logger.Error(res.ErrorMsg);
                        //                            }
                        //                            ErrorLog(t, 1, rjson, "");
                        //                        }
                        //                    }
                        //                }
                        //                break;
                        //            }
                        //            #endregion

                        //    }
                        //});
                        #endregion
                        foreach (var t in list)
                        {

                            var json = "";
                            switch (t.SqlId)
                            {
                                case "1":

                                    #region 小件录入节点推送
                                    {
                                        var data = PushDataDal.Instance.GetPushData<EpcAscanModel.TrackingModel>(item.SqlContent, t.Number1).FirstOrDefault();
                                        if (data == null)
                                        {
                                            json = "";
                                        }
                                        else
                                        {
                                            data.in_date = data.in_date.Replace("/", "-");
                                            var mo = new EpcAscanModel()
                                            {
                                                api_key = ApiKey,
                                                data = new List<EpcAscanModel.TrackingModel>() { data },
                                            };
                                            json = mo.ToJsonString();
                                        }
                                        if (string.IsNullOrWhiteSpace(json))
                                        {
                                            LogMsg(t, 0, "", "", "没有获取到Json数据", "A-SCAN");
                                        }
                                        else
                                        {
                                            var urie = ApiUrl + $"/{item.EventCode}";
                                            var result = PostApi<EpcAscanResponseModel>(t, urie, json);
                                            if (result != null)
                                            {
                                                var rjson = result.ToJsonString();
                                                if (result.data[0].message != "Success")
                                                {
                                                    LogMsg(t, 0, json, rjson, $"WISH接口返回错误：{result.data[0].message}", "A-SCAN");
                                                }
                                                else //成功修改状态
                                                {
                                                    LogMsg(t, 1, json, rjson, "A-SCAN推送成功", "A-SCAN");
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion

                                case "2":
                                case "3"://立即发货

                                    #region 合包
                                    {
                                        //合包明细
                                        try
                                        {
                                            List<string> orders;
                                            if (t.SqlId == "3")
                                            {
                                                orders = new List<string>() { t.Number2.Replace("\"", "") };
                                            }
                                            else
                                            {
                                                orders = JsonConvert.DeserializeObject<List<string>>(t.Number2);
                                            }

                                            //如果已经有合包的件则单独去掉
                                            var combinedlist = EpcDal.Instance.GetCombinedNumList(orders);
                                            var orderlist = orders.Except(combinedlist).ToList();
                                            var data = EpcDal.Instance.GetCombineOrderModel(orderlist);
                                            if (orderlist.Any())
                                            {
                                                data.api_key = ApiKey;
                                                data.orders = orderlist.ToArray();
                                                data.total_weight = (float)(EpcDal.Instance.GetEpcCombineWeight(orderlist, t.Parameter) * 0.001);
                                                json = data.ToJsonString();
                                            }
                                            else
                                            {
                                                if (orders.Any())
                                                {
                                                    LogMsg(t, 1, json, "", $"内置小件有部分已推送过,原单号{orders.Count},", "CombinedPackage");
                                                    continue;
                                                }
                                                json = "";
                                            }
                                            if (string.IsNullOrWhiteSpace(json))
                                            {
                                                LogMsg(t, 0, "", "", "没有获取到Json数据", "CombinedPackage");
                                            }
                                            else
                                            {
                                                var urie = ApiUrl + $"/{item.EventCode.Trim()}";
                                                var result = PostApi<EpcCombineResponseModel>(t, urie, json);
                                                if (result != null)
                                                {
                                                    var rjson = result.ToJsonString();
                                                    if (result.status != "Success")
                                                    {
                                                        LogMsg(t, 0, json, rjson, $"WISH接口返回错误：{result.message}", "CombinedPackage");
                                                    }
                                                    else //成功修改状态
                                                    {
                                                        if (t.SqlId == "3")
                                                        {
                                                            t.Parameter = "4";//立即发货无包材
                                                        }
                                                        var res = PushDataDal.Instance.SaveCombineResponse(t, json, rjson, data.orders.ToList());
                                                        if (res.HasError)
                                                        {
                                                            LogMsg(t, 0, json, rjson, $"保存合包返回结果出错：{res.ErrorMsg}", "CombinedPackage");
                                                        }
                                                        else
                                                        {
                                                            LogMsg(t, 1, json, rjson, "保存合包返回结果成功", "CombinedPackage");
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            LogMsg(t, 0, json, "", $"代码逻辑出错！原因{e.Message}明细{e.InnerException}", "CombinedPackage");
                                        }
                                        break;
                                    }
                                #endregion

                                case "4": //发运节点
                                    var model = JsonConvert.DeserializeObject<EpcSendRequestModel>(t.Number2);
                                    if (model == null)
                                    {
                                        LogMsg(t, 0, t.Number2, "", $"[发运]数据格式错误", "Shipment");
                                    }
                                    else
                                    {
                                        try
                                        {
                                            model.api_key = ApiKey;
                                            model.trackings[0].checkpoints[0].timestamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                                            json = model.ToJsonString();
                                            var urie = ApiUrl + $"/{item.EventCode}";
                                            var result = PostApi<EpcSendRequestModel.EpcSendResultModel>(t, urie, json);
                                            if (result != null)
                                            {
                                                var rjson = result.ToJsonString();
                                                if (result.msg != "Success")
                                                {
                                                    LogMsg(t, 0, t.Number2, rjson, $"WISH接口返回错误:{result.msg}", "Shipment");
                                                }
                                                else //成功修改状态
                                                {
                                                    LogMsg(t, 1, json, rjson, "发运推送成功", "Shipment");
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            LogMsg(t, 0, json, "", $"调取WISH发运接口出错！原因{e.Message}明细{e.InnerException}", "Shipment");
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    Thread.Sleep(int.Parse(DelayMinutes) * 1000);
                }
                Thread.Sleep(int.Parse(DelayMinutes) * 1000);
            }
            catch (Exception ex)
            {
                Logger.Error("接口出错" + ex.Message + ex.InnerException);
                Thread.Sleep(int.Parse(DelayMinutes) * 1000);
            }

        }

        ///// <summary>
        ///// 修改推送状态是否成功 
        ///// </summary>
        ///// <param name="t"></param>
        ///// <param name="type">0 修改失败 1修改成功</param>
        ///// <param name="json">json数据</param>
        ///// <param name="msg">首要错误信息</param>
        //public void ErrorLog(PushDataMethodsModel t, int type, string json, string msg)
        //{
        //    if (!string.IsNullOrWhiteSpace(msg))
        //    {
        //        Logger.Error($"SqlId[{ t.SqlId}],单号[{ t.Number1}]{msg}");
        //        PushDataDal.Instance.SavePushMessage(0, t.Number1, msg, "[dbo].[EPC_PushDataMethods]", json);
        //    }
        //    var badState = PushDataDal.Instance.UpState(t, type, "[dbo].[EPC_PushDataMethods]");
        //    if (badState.HasError)
        //    {
        //        Logger.Error($"单号[{t.Number1}]修改推送状态时出错" + badState.ErrorMsg);
        //        return;
        //    }
        //    if (type == 1)
        //    {
        //        Logger.Info($"SqlId[{t.SqlId}],单号[{t.Number1}]推送成功");
        //        PushDataDal.Instance.SavePushMessage(1, t.Number1, msg, "EPC_CombineResponse", json);
        //    }
        //}


        /// <summary>
        /// 修改推送状态
        /// </summary>
        /// <param name="t"></param>
        /// <param name="type">0 失败 1成功</param>
        /// <param name="qjson">请求json</param>
        /// <param name="fjson">返回json</param>
        /// <param name="msg">错误信息</param>
        /// <param name="node"></param>
        public void LogMsg(PushDataMethodsModel t, int type, string qjson, string fjson, string msg, string node)
        {
            var comstr = $"SqlId[{ t.SqlId}],单号[{ t.Number1}]";
            if (type == 0)//失败
            {
                comstr += $"失败！原因:[{msg}]   请求json：{qjson}   返回json：{fjson}";
                Logger.Error(comstr);

                PushDataDal.Instance.SavePushMessage(new PushMessageModel { Type = "0",TaskId = t.Id,Number = t.Number1, Msg = msg, Table = "[dbo].[EPC_PushDataMethods]", Rjson = qjson, Fjson = fjson, Node = node });
            }
            if (type == 1)
            {
                comstr += $"成功!";
                Logger.Info(comstr);
                PushDataDal.Instance.SavePushMessage(new PushMessageModel { Type = "0",TaskId = t.Id,Number = t.Number1, Msg = msg, Table = "[dbo].[EPC_PushDataMethods]", Rjson = qjson, Fjson = fjson, Node = node });
            }
            var badState = PushDataDal.Instance.UpState(t, type, "[dbo].[EPC_PushDataMethods]");
            if (badState.HasError)
            {
                Logger.Error($"[{comstr}]修改推送状态时出错" + badState.ErrorMsg);
                return;
            }
        }

        /// <summary>
        /// 请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="uri"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public T PostApi<T>(PushDataMethodsModel t, string uri, string json)
        {
            try
            {
                var res = CommonApi.Instance.Post(uri, json);
                if (res.HasError)
                {
                    //LogMsg(t, 0, json, $"调取WISH接口出错！详情：{res.ErrorMsg}请求JSON：\r\n{json}\r\n返回Json:\r\n {res.Data}");
                    return default(T);
                }
                Logger.Info($"SqlId[{ t.SqlId}],单号[{ t.Number1}]返回Json:" + res.Data + "\r\n");
                var result = JsonConvert.DeserializeObject<T>(res.Data);
                return result;
            }
            catch (Exception ex)
            {
                //ErrorLog(t, 0, json, $"调取WISH接口出错！详情：{ex.Message} 明细{ex.InnerException}请求JSON：" + json);
                return default(T);
            }

        }
    }
}
