using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Yanwen.EPC.Common.Model;
using Yanwen.TMS.Base.DataAccess;

namespace YanWen.EPC.Push.Common.DataAccess
{
    public class EpcMqPushTakDal : BaseDal
    {

        public static readonly Lazy<EpcMqPushTakDal> Lazy = new Lazy<EpcMqPushTakDal>(() => new EpcMqPushTakDal());
        public static EpcMqPushTakDal Instance => Lazy.Value;

        public void SaveMaTask(string number, string mainPart, string action, string dbAction)
        {
            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                string sql = "Insert into [dbo].[EPC_MqPushTask] ([Number],[MainPart],[Action],[DbAction])values (@number,@mainPart,@action,@DbAction) ";
                conn.Execute(sql, new { number, mainPart, DbAction = dbAction, action });
            }
        }

        public MqPushObjectModel GetExpressByWaybillNumber(string trackingNumber)
        {
            var sql = "select * from EPC_Express where TrackingNumber=@TrackingNumber";
            using (IDbConnection conn = OpenConnection("Yanwen_EPC"))
            {
                return new MqPushObjectModel()
                {
                    EpcExpress = conn.Query<EpcExpressModel>(sql, new { TrackingNumber = trackingNumber }).FirstOrDefault()
                };
            }
        }

        /// <summary>
        /// 获取合包推送MQ
        /// </summary>
        /// <param name="barCode"></param>
        /// <returns></returns>
        public MqPushObjectModel GetCombinedPackageByBarCode(string barCode)
        {
            var sql = @" select distinct t.* ,t2.ServiceCode,t2.ProductCode,t2.WarehouseId,t2.CustomerCode,getdate() as UpdateTime from  [EPC_CombinedPackage]  t 
  inner join [dbo].[EPC_CombinedPackageDetail] t1 on t.BarCode =t1.BarCode 
  inner join EPC_Express t2 on t2.TrackingNumber =t1.TrackingNumber 
  where  t1.BarCode=@BarCode";
            string sqld = " select TrackingNumber from [dbo].[EPC_CombinedPackageDetail] where BarCode=@BarCode and IsDelete=0 group by TrackingNumber";
            using (IDbConnection conn = OpenConnection("Yanwen_EPC"))
            {
                var data = new MqPushObjectModel()
                {
                    EpcCombinedPackage = conn.Query<MqPushObjectModel.EpcCombinedPackageModel>(sql, new { BarCode = barCode }).FirstOrDefault(),
                };
                data.EpcCombinedPackage.ProductCode = "725";//此处产品号用于包材的计费              
                data.EpcCombinedPackage.OrderList = conn.Query<string>(sqld, new { BarCode = barCode }).ToList();
                return data;
            }
        }

        public void InsertPush(MqPushModel model)
        {
            using (IDbConnection conn = OpenConnection())
            {
                var sql = @"insert into push.MQPushTask(Number,Topic,Tag,Body) values(@Number,@Topic,@Tag,@Body)";
                conn.Execute(sql, new { Number = model.Number, Topic = model.Topic, Tag = model.Tag, Body = model.Body });
            }
        }
    }
}
