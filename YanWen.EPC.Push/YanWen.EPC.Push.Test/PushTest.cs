using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Yanwen.Framework.Core.Extensions;
using YanWen.EPC.Push.Common.Api;
using YanWen.EPC.Push.Common.Model;
using YanWen.EPC.Push.Test.Models;

namespace YanWen.EPC.Push.Test
{
    [TestFixture]
    public class PushTest
    {



        [Test]
        public void Shipment()
        {
            var s1 = DateTime.Parse("2019-02-27 20:17:13.000").AddHours(168).ToString("yyyy-MM-dd");
            var s = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var jsondata = YanWen.EPC.Push.Common.DataAccess.EpcDal.Instance.GetPushData();
            var list = JsonConvert.DeserializeObject<List<PushModel>>(jsondata);
            var ApiKey = "JHBia2RmMiQxMDAkUW1ndEJRQ2c5RjdMV2V0OUwwWG9uUSRCQ2l5cXZnWDRZUEdETzB0RDh2YVBjNVBhOGs=";
            var ApiUrl = "https://wishpost.wish.com/api/v1";
            var i = 0;
            foreach (var t in list)
            {
                var model = JsonConvert.DeserializeObject<EpcSendRequestModel>(t.PushJson);
                if (model == null)
                {
                    Console.WriteLine("数据格式错误");
                }
                else
                {
                    try
                    {
                        model.api_key = ApiKey;
                        model.trackings[0].checkpoints[0].timestamp = DateTime.Parse(t.CreateTime).ToString("yyyy-MM-dd hh:mm:ss");
                        var json = model.ToJsonString();
                        model.trackings[0].checkpoints[0].timestamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        var urie = ApiUrl + "/tracking-webhook";
                        var result = PostApi<EpcSendRequestModel.EpcSendResultModel>(urie, json);
                        if (result != null)
                        {
                            var rjson = result.ToJsonString();
                            if (result.msg != "Success")
                            {
                                Console.WriteLine($"单号：{t.BarCode}WISH接口返回错误{result.msg}");
                            }
                            else //成功修改状态
                            {
                                i++;
                                //Console.WriteLine("发运推送成功");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("代码错误");

                    }
                }
            }

            Console.WriteLine($"成功{i}");
        }


        public T PostApi<T>(string uri, string json)
        {
            try
            {
                var res = CommonApi.Instance.Post(uri, json);
                if (res.HasError)
                {
                    //LogMsg(t, 0, json, $"调取WISH接口出错！详情：{res.ErrorMsg}请求JSON：\r\n{json}\r\n返回Json:\r\n {res.Data}");
                    return default(T);
                }
                var result = JsonConvert.DeserializeObject<T>(res.Data);
                return result;
            }
            catch (Exception ex)
            {
                //ErrorLog(t, 0, json, $"调取WISH接口出错！详情：{ex.Message} 明细{ex.InnerException}请求JSON：" + json);
                return default(T);
            }

        }

        [Test]
        public void DingDing()
        {
            var url = "https://oapi.dingtalk.com/robot/send?access_token=5f534e705e8965260dbae62e29c9abc7b5bdb9ff5c43af1914426ac2d8516d81";
            var mo = new DingDingMsgModel()
            {
                msgtype = "text",
                text = new TextModel()
                {
                    content = "testdasdas大萨达所",
                },
                at = new At()
                {
                    atMobiles = new[] { "18731192118" },
                    isAtAll = false
                },
            };
            string response = HttpPostConnectToServer(url, mo.ToJsonString());
        }

        [Test]
        public void DingDingMarkdown()
        {
            var list = new List<DingDingMsgModel>()
            {
                new DingDingMsgModel()
                {
                    msgtype = ""
                },
                new DingDingMsgModel()
                {
                msgtype = null
            }
            };

            var l = list.Where(t => t.msgtype.Contains("")).ToList();
            var url = "https://oapi.dingtalk.com/robot/send?access_token=5f534e705e8965260dbae62e29c9abc7b5bdb9ff5c43af1914426ac2d8516d81";
            var oper = "1001";
            var com = "01";
            var mo = new MarkdownModel()
            {
                msgtype = "markdown",
                //markdown = new Markdown()
                //{
                //    text = $@"<font color=#FF0000 size=11 face='黑体'>事故系统[TMS] </font>   
                //    <font color=black size=6 >**发生时间:** {DateTime.Now} </font>   
                //    <font color=black size=6 >**涉及页面:** 录入  </font>   
                //    <font color=black size=6 >**操作人:** {oper}  </font>  
                //    <font color=black size=6 >**操作仓:** {com}   </font>  
                //    <font color=black size=6 >**问题:** 就是有问题  </font>",
                //    title = "收到TMS错误信息",
                //},
                markdown = new Markdown()
                {
                    text = $@"<font color=#FF0000 size=11 face='黑体'>事故系统[TMS]</font>    
**发生时间:** {DateTime.Now}    
**涉及页面:** 录入    
**操作仓:** {com}   
**操作人:** {oper}   
**问题:** 就是有问题",
                    title = "收到TMS错误信息",
                },
                at = new At()
                {
                    atMobiles = new[] { "" },
                    isAtAll = false
                },
            };
            string response = HttpPostConnectToServer(url, mo.ToJsonString());
            Console.WriteLine(response);
        }

        public string GetTimeStamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)System.Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime.ToString();
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        private static string HttpPostConnectToServer(string serverUrl, string postData)
        {
            var dataArray = Encoding.UTF8.GetBytes(postData);
            //创建请求 
            var request = (HttpWebRequest)HttpWebRequest.Create(serverUrl);
            request.Method = "POST";
            request.ContentLength = dataArray.Length;
            //设置上传服务的数据格式 
            request.ContentType = "application/json";
            //请求的身份验证信息为默认 
            request.Credentials = CredentialCache.DefaultCredentials;
            //请求超时时间 
            request.Timeout = 3600;
            //创建输入流 
            Stream dataStream;
            try
            {
                dataStream = request.GetRequestStream();
            }
            catch (Exception)
            {
                return null;//连接服务器失败 
            }
            //发送请求 
            dataStream.Write(dataArray, 0, dataArray.Length);
            dataStream.Close();
            //读取返回消息 
            string res = "";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                res = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception ex)
            {
                return "调取供应商接口时出错！";
            }
            return res;
        }
    }
}
