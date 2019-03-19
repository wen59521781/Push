using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Yanwen.Framework.Core.Extensions;
using Yanwen.TMS.Base;

namespace YanWen.EPC.Push.Common.Api
{
    public class CommonApi
    {

        public static readonly Lazy<CommonApi> Lazy = new Lazy<CommonApi>(() => new CommonApi());

        private static readonly string ApiUrl = ConfigurationManager.AppSettings["BiSaveNumber"];
        public static CommonApi Instance => Lazy.Value;

        /// <summary>  
        /// 指定Post地址使用Get 方式获取全部字符串  
        /// </summary>  
        /// <param name="url">请求后台地址</param>
        /// <param name="jsonData"></param>
        /// <returns></returns>  
        public Result<string> Post(string url, string jsonData)
        {
            var result = new Result<string> { HasError = false };
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                //req.ProtocolVersion = HttpVersion.Version10;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) => true);
                req.Method = "POST";
                req.ContentType = "application/json;charset=UTF-8";
                #region 添加Post 参数  
                byte[] data = Encoding.UTF8.GetBytes(jsonData);
                req.ContentLength = data.Length;

                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
                #endregion
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                //获取响应内容  
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result.Data = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorMsg = e.Message + e.InnerException;
                return result;
            }

            return result;
        }

        /// <summary>  
        /// </summary> 
        /// <param name="url">调用的Api地址</param> 
        /// <param name="requestJson">表单数据（json格式）</param> 
        /// <returns></returns> 
        public Result<string> PostResponseJson(string url, string requestJson)
        {
            var result = new Result<string> { HasError = false };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpContent httpContent = new StringContent(requestJson);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;
            if (response.IsSuccessStatusCode)
            {
                string responseJson = response.Content.ReadAsStringAsync().Result;
                result.Data = responseJson;
                return result;
            }
            result.HasError = true;
            result.ErrorMsg = "出错了,StatusCode:" + response.StatusCode.ToString();
            return result;
        }


        public static T FromJson<T>(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? default(T) : JsonConvert.DeserializeObject<T>(s);
        }

        /// <summary>
        /// 保存BI单号
        /// </summary>
        /// <param name="eventCode">时间COde</param>
        /// <param name="sourceId">燕文平台号</param>
        /// <param name="waybillnumber">运单号</param>
        /// <param name="msgcode">返回Code单号</param>
        /// <returns></returns>
        public Result<bool> SaveBiNumber(string eventCode, string sourceId, string waybillnumber, string msgcode)
        {
            var result = new Result<bool> { HasError = false };
            try
            {
                var model = new List<BiModel>
                {
                    new BiModel
                    {
                        msgcode = msgcode,
                        waybillnumber = waybillnumber,
                        SourceId = sourceId,
                        eventCode = eventCode
                    }
                };
                var urie = new Uri(ApiUrl);
                result = urie.PostData<Result<bool>>(model.ToJsonString());
                return result;
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorMsg = "保存燕文单号时出错！详情：" + e.Message;
                return result;
            }

        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public  string HttpPostConnectToServer(string serverUrl, string postData)
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

    public class BiModel
    {
        public string msgcode { get; set; }
        public string waybillnumber { get; set; }
        public string SourceId { get; set; }
        public string eventCode { get; set; }
    }

}
