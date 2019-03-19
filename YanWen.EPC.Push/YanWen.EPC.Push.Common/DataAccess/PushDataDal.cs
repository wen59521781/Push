using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Yanwen.Framework.Core.Extensions;
using Yanwen.TMS.Base;
using Yanwen.TMS.Base.DataAccess;
using YanWen.EPC.Push.Common.Model;

namespace YanWen.EPC.Push.Common.DataAccess
{
    public class PushDataDal : BaseDal
    {
        public static readonly Lazy<PushDataDal> Lazy = new Lazy<PushDataDal>(() => new PushDataDal());
        public static PushDataDal Instance => Lazy.Value;


        /// <summary>
        /// 查询出所有的SQLid
        /// </summary>
        /// <returns></returns>
        public List<PushSqlSentenceModel> GetAllPushSql()
        {
            string sql = @"select  * from   [dbo].[EPC_PushSqlContent]";

            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                return conn.Query<PushSqlSentenceModel>(sql).ToList();
            }
        }

        /// <summary>
        /// 获取推送单号
        /// </summary>
        /// <param name="recordsNumber">TOP</param>      
        /// <param name="failPushNumber">不超过的错误次数</param>
        /// <param name="sqlId">sqlId</param>
        /// <returns></returns>
        public List<PushDataMethodsModel> GetPushList(int recordsNumber, int failPushNumber, string sqlId)
        {
            var sql = "select  top (@recordsNumber) * from  [dbo].[EPC_PushDataMethods]  where StatusId =0 and Sqlid=@Sqlid and PushNum<@failPushNumber  order by  PushNum";
            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                return conn.Query<PushDataMethodsModel>(sql, new { recordsNumber, failPushNumber, Sqlid = sqlId }).ToList();
            }
        }

        /// <summary>
        /// 取数据直接转Json
        /// </summary>
        /// <param name="model"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public string GetPushDataForJson(PushSqlSentenceModel model, string parameter)
        {
            string sql = model.SqlContent;
            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                var data = conn.Query(sql, new { number1 = parameter }).ToList();
                return data.Any() ? data.ToJsonString().Trim('[', ']') : "";
            }
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlContent"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public List<T> GetPushData<T>(string sqlContent, string parameter)
        {
            string sql = sqlContent;
            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                return conn.Query<T>(sql, new { number1 = parameter }).ToList();
            }
        }

        /// <summary>
        /// 保存合包接口返回的数据
        /// </summary>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="rjson"></param>
        /// <param name="orderList"></param>
        /// <returns></returns>
        public Result<bool> SaveCombineResponse(PushDataMethodsModel model, string json, string rjson, List<string> orderList)
        {
            var result = new Result<bool>() { HasError = false };
            try
            {
                var sql = "insert [dbo].[EPC_CombineResponse] ([Number],[ResponseJson],[RequestJson],Parameter)values(@Number,@ResponseJson,@RequestJson,@Parameter)";
                sql += @" update [EPC_OrderDetail] set status =4 where  first_mile_tracking_id in @orderList;
                          update EPC_Express set Status =4 where TrackingNumber in @orderList";//推送成功之后修改订单状态
                using (var conn = OpenConnection("Yanwen_EPC"))
                {
                    if (string.IsNullOrWhiteSpace(model.Parameter) || model.Parameter.ToUpper() == "NULL")
                    {
                        model.Parameter = "4";
                    }
                    result.Data = conn.Execute(sql, new { Number = model.Number1, ResponseJson = rjson, RequestJson = json, Parameter = model.Parameter, orderList }) > 0;
                    if (!result.Data)
                    {
                        result.HasError = true;
                        result.ErrorMsg = $"保存合包JSON失败修改行数为0！SQLId[{model.SqlId}],Id[{model.Id}]单号[{model.Number1}]";
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorMsg = $"保存合包JSON代码异常！SQLId[{model.SqlId}],Id[{model.Id}]单号[{model.Number1}]详情：" + e.Message;
                return result;
            }
            return result;
        }


        /// <summary>
        /// 修改状态
        /// </summary>
        /// <param name="model"></param>
        /// <param name="type"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public Result<bool> UpState(PushDataMethodsModel model, int type, string table)
        {
            var result = new Result<bool>() { HasError = false };
            try
            {
                var sql = "";
                if (type == 0)//失败
                {
                    sql = $"update {table} set PushNum = PushNum+1 where Id=@Id and SqlId=@SqlId  and StatusId=0";
                }
                if (type == 1)//成功
                {
                    sql = $"update {table} set StatusId=1 where Id=@Id and SqlId=@SqlId  and StatusId=0";
                }
                if (type == 2)//MQ推送成功
                {
                    sql = $"update {table} set StatusId=1 where Id=@Id and StatusId=0";
                }
                if (type == 3)//MQ推送失败
                {
                    sql = $"update {table} set PushNum = PushNum+1  where Id=@Id and StatusId=0";
                }
                using (var conn = OpenConnection("Yanwen_EPC"))
                {
                    result.Data = conn.Execute(sql, new { model.Id, model.SqlId }) > 0;
                    if (!result.Data)
                    {
                        result.HasError = true;
                        result.ErrorMsg = $"更新状态没有成功！SQLId[{model.SqlId}],Id[{model.Id}]单号[{model.Number1}]";
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorMsg = "更新状态出错！详情：" + e.Message;
                return result;
            }
            return result;
        }


        /// <summary>
        /// 获取推送单号
        /// </summary>
        /// <param name="recordsNumber">TOP</param>      
        /// <param name="failPushNumber">不超过的错误次数</param>      
        /// <returns></returns>
        public List<CombineResponsePushModel> GetPushList(int recordsNumber, int failPushNumber, int type)
        {
            string sql = "";
            if (type == 0)
            {
                sql = "select  top (@recordsNumber) * from  [dbo].[EPC_CombineResponse]  where StatusId =0  and PushNum<@failPushNumber  order by  PushNum";
            }
            else
            {
                sql = "select  top (@recordsNumber) * from  [dbo].[EPC_MqPushTask]  where StatusId =0  and PushNum<@failPushNumber  order by  PushNum";
            }

            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                return conn.Query<CombineResponsePushModel>(sql, new { recordsNumber, failPushNumber }).ToList();
            }
        }


        public Result<bool> UpState(CombineResponsePushModel model, int type)
        {
            var result = new Result<bool>() { HasError = false };
            try
            {
                var sql = "";
                if (type == 0)//失败
                {
                    sql = $"update EPC_CombineResponse set PushNum = PushNum+1 where Id=@Id  and StatusId=0";
                }
                if (type == 1)//成功
                {
                    sql = $"update EPC_CombineResponse set StatusId=1 where Id=@Id   and StatusId=0";
                }
                using (var conn = OpenConnection("Yanwen_EPC"))
                {
                    result.Data = conn.Execute(sql, new { model.Id }) > 0;
                    if (!result.Data)
                    {
                        result.HasError = true;
                        result.ErrorMsg = $"更新状态没有成功！Id[{model.Id}]单号[{model.Number}]";
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorMsg = "更新状态出错！详情：" + e.Message;
                return result;
            }
            return result;
        }

        /// <summary>
        /// 保存请求信息
        /// </summary>    
        /// <returns></returns>
        public Result<bool> SavePushMessage(PushMessageModel model)
        {
            var result = new Result<bool>() { HasError = false };
            try
            {
                string sql = @"delete [EPC_PushMessage] where Number= @number and taskId=@taskId and PushTable=@table;
                              insert into [EPC_PushMessage] ([Number],[TaskId],[Message],[PushTable],[RequestJson],[ResponseJson],[Node])
                              values(@number,@TaskId,@msg,@table,@rjson,@ResponseJson,@Node)";
                //if (type == 1)
                //{
                //    sql = " delete [EPC_PushMessage] where Number= @number and PushTable=@table;";
                //}

                using (var conn = OpenConnection("Yanwen_EPC"))
                {
                    result.Data = conn.Execute(sql, new { model.Number, TaskId = model.TaskId, model.Msg, model.Table, model.Rjson, ResponseJson = model.Fjson, Node = model.Node }) > 0;
                }
            }
            catch (Exception r)
            {
                return result;
            }
            return result;
        }


        /// <summary>
        /// 获取推送失败的TaskID
        /// </summary>
        /// <returns></returns>
        public List<string> GetBadPushList()
        {
            string sql = "select id from EPC_PushDataMethods where StatusId =0 and PushNum =15";
            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                var list = conn.Query<string>(sql).ToList();
                return list;
            }
        }
    }
}