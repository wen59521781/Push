using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yanwen.TMS.Base;
using Yanwen.TMS.Common.ApiModel;
using YanWen.Handle.Model.WintModel;

namespace YanWen.Handle.Api
{
    public class WintApi
    {
        private static string winitApiBaseUrl = ConfigurationManager.AppSettings["winit.ApiBaseUrl"];
        private static string winitApiAppkey = ConfigurationManager.AppSettings["winit.Appkey"];
        private static string winitApiToken = ConfigurationManager.AppSettings["winit.Token"];


        public static readonly Lazy<WintApi> Lazy = new Lazy<WintApi>(() => new WintApi());
        public static WintApi Instance => Lazy.Value;

        /// <summary>
        /// 生成WINIT加密字符串
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        private string GetWinitSign(dynamic jsonData, string actionName)
        {
            string sortString = $"{winitApiToken}action{actionName}app_key{winitApiAppkey}data{jsonData}formatjsonplatformsign_methodmd5timestampversion1.0{winitApiToken}";
            return GetMD5Key(sortString);
        }

        /// <summary>
        /// 加密方法
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetMD5Key(string key)
        {
            byte[] arry = Encoding.UTF8.GetBytes(key);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(arry);
            string result = BitConverter.ToString(output).Replace("-", "");
            return result;
        }

        /// <summary>
        /// 万邑通回传Bam数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Result<Result> PustWintPackageBam(WintBamModel model)
        {
            var result = new Result<Result>() { HasError = false };
            var response = PostWinitData(model, "isp.order.ywBigPackage");
            if (response.Contains("调万邑通接口出错"))
            {
                result.HasError = true;
                result.ErrorMsg = response;
                return result;
            }
            result.Data = JsonConvert.DeserializeObject<Result>(response);
            return result;
        }

        public string PostWinitData(object model, string actionName)
        {
            var modelJson = JsonConvert.SerializeObject(model);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sortString = string.Format("{0}action{1}app_key{2}data{3}format{4}platform{5}sign_method{6}timestamp{7}version1.0{0}", winitApiToken, actionName, winitApiAppkey, modelJson, "json", "yanwen", "md5", "timestamp");
            var postData = new
            {
                action = actionName,
                app_key = winitApiAppkey,
                data = model,
                format = "json",
                language = "zh_CN",
                platform = "yanwen",
                sign = GetMD5Key(sortString),
                sign_method = "md5",
                timestamp = "timestamp",
                version = "1.0",
            };          
            string postJsonString = JsonConvert.SerializeObject(postData);
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers.Add("Content-Type", "Application/json; charset=utf-8");
                try
                {
                    var postResult = client.UploadString(winitApiBaseUrl, postJsonString);
                    return postResult;
                }
                catch (Exception ex)
                {
                    return "调万邑通接口出错！详情:" + ex.Message;
                }
            }
        }

    }
}
