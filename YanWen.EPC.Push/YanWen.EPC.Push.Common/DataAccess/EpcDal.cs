using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Yanwen.EPC.Common.DataAccess;
using Yanwen.EPC.Common.Logic;
using Yanwen.Framework.Core.Extensions;
using Yanwen.TMS.Base;
using Yanwen.TMS.Base.DataAccess;
using Yanwen.TMS.Common.Model;
using YanWen.EPC.Push.Common.Model;

namespace YanWen.EPC.Push.Common.DataAccess
{
    public class EpcDal : BaseDal
    {
        public static readonly Lazy<EpcDal> Lazy = new Lazy<EpcDal>(() => new EpcDal());
        public static EpcDal Instance => Lazy.Value;

        /// <summary>
        /// 保存到合包表
        /// </summary>
        /// <returns></returns>
        public Result<bool> SaveEpcCombine(EpcCombineModel model, List<string> orders)
        {
            var result = new Result<bool>() { HasError = false };
            string sql = @"insert into [EPC_CombinedPackage]([BarCode],[ExchangeNumber],[CombineId],[pdf_10_lcl_url],[pdf_a4_en_url],[pdf_a4_lcl_url],[pdf_10_en_url],
[label_format],[carrier],[Weight],[Piece],[PackageLabelsType],[CreateTime])values (@BarCode,@ExchangeNumber,@CombineId,@pdf_10_lcl_url,@pdf_a4_en_url,@pdf_a4_lcl_url,@pdf_10_en_url,
@label_format,@carrier,@Weight,@Piece,@PackageLabelsType,getdate())";
            string sql1 = @"insert into  [dbo].[EPC_CombinedPackageDetail] (BarCode,ExchangeNumber,TrackingNumber) values(@BarCode,@ExchangeNumber,@TrackingNumber)";
            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                try
                {
                    var count = conn.Execute(sql, new
                    {
                        BarCode = model.BarCode,
                        ExchangeNumber = model.ExchangeNumber,
                        CombineId = model.CombineId,
                        pdf_10_lcl_url = model.pdf_10_lcl_url,
                        pdf_a4_en_url = model.pdf_a4_en_url,
                        pdf_a4_lcl_url = model.pdf_a4_lcl_url,
                        pdf_10_en_url = model.pdf_10_en_url,
                        label_format = model.label_format,
                        carrier = model.carrier,
                        Weight = model.Weight,
                        PackageLabelsType = model.PackageLabelsType,
                        Piece = model.Piece
                    }) > 0;
                    foreach (string t in orders)
                    {
                        //保存明细
                        conn.Execute(sql1, new { BarCode = model.BarCode, ExchangeNumber = model.ExchangeNumber, TrackingNumber = t });
                    }
                    //保存到MQ任务表
                    EpcMqPushTakDal.Instance.SaveMaTask(model.BarCode, "EPC_CombinedPackage", "EpcCombinedPackage", "Add");
                    if (!count)
                    {
                        result.HasError = true;
                        result.ErrorMsg = "修改行数返回为0";
                        return result;
                    }
                }
                catch (Exception e)
                {
                    result.HasError = true;
                    result.ErrorMsg = "执行语句时出错！详情:" + e.Message;
                    return result;
                }
            }
            return result;
        }

        /// <summary>
        /// 根据单号汇总重量(单位G)
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="labeltype"></param>
        /// <param name="packagingweight"></param>
        /// <returns></returns>
        public int GetEpcCombineWeight(List<string> orders, string labeltype, string packagingweight)
        {
            if (string.IsNullOrWhiteSpace(labeltype))
            {
                labeltype = "4";
            }
            if (!"1,2,3,4,5".Contains(labeltype))
            {
                labeltype = "4";
            }
            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                string sql = @"select  SUM(TrueWeight) from EPC_Express where TrackingNumber in @orders";
                string wsql = @"select  LabelWeight+PackingWeight from [dbo].[BS_PackingType] where CompanyCode ='10' and PackageLabelsType=@labeltype";
                var label = conn.Query<int>(wsql, new { orders = orders, labeltype }).FirstOrDefault();
                if (label <= 0)
                {
                    label = 1;
                }
                var weight = conn.Query<int>(sql, new { orders = orders, labeltype }).FirstOrDefault();
                var sumWeight = label + weight;
                if (labeltype == "5")//手填的重量
                {
                    sumWeight = sumWeight + int.Parse(packagingweight);
                }
                return sumWeight;
                //if (sumWeight >= 2000)
                //{
                //    return 1999;
                //}
                //else
                //{
                //    return sumWeight;
                //}
            }
        }

        /// <summary>
        /// 根据单号汇总获取申报价值
        /// </summary>
        /// <returns></returns>
        public EpcCombineOrderModel GetCombineOrderModel(List<string> orders)
        {
            var mo = new EpcCombineOrderModel() { };
            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                string sqlhas = @"select  * from EPC_Express where TrackingNumber in @orders";
                var list = conn.Query<EpcExpressModel>(sqlhas, new { orders = orders }).ToList();
                if (list.Any())
                {
                    var value = list.Sum(t => t.Value);
                    mo.total_value = value;
                    mo.orders = orders.ToArray();
                    mo.has_battery = list.Any(t => t.IsBattery);
                    mo.has_liquid = list.Any(t => t.IsLiquid);
                    mo.has_sensitive = list.Any(t => t.IsSensitive);
                    mo.operator_id = "1001";
                    return mo;
                }
                return null;
            }
        }

        /// <summary>
        /// 非燕文货物也存临时表
        /// </summary>
        /// <param name="model"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        public Result<bool> SaveExpress(EpcCombineModel model, List<string> orders)
        {
            var result = new Result<bool>() { HasError = false };
            var value = GetCombineOrderModel(orders).total_value;
            var carrierModel = GetCarrier(model.carrier.ToString(), 0);
            if (carrierModel == null)
            {
                result.HasError = true;
                result.ErrorMsg = $"没有找到对应carrier[{model.carrier}]的服务配置！请配置";
                return result;
            }
            using (var conn = OpenConnection())
            {
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        var express = new ExpressModel() { };
                        express.WaybillNumber = model.BarCode;
                        express.OrderCode = "";
                        express.ExchangeNumber = model.BarCode;
                        express.Piece = orders.Count;
                        express.Weight = model.Weight;
                        express.SupplierWeight = model.Weight;
                        express.TrueWeight = model.Weight;
                        express.ApSpecialAmount = value;
                        express.ApDeclaredValue = value;
                        express.ArDeclaredValue = value;
                        express.ArDeclaredValue = value;
                        express.CompanyCode = "10";
                        express.SourceId = 12;
                        express.CreateId = "92315";
                        express.CustomerCode = 30045174;
                        express.RegionId = 2860;
                        express.PostCode = "523000";
                        express.CreateTime = DateTime.Now;
                        express.AscanTime = DateTime.Now;
                        express.ReceiveDate = DateTime.Now;
                        express.PlatformOrderTime = DateTime.Now;
                        express.EPReceiveDate = DateTime.Now;
                        express.CustomerDiscountLevel = "0";
                        express.AreaCode = "1003";

                        express.ServiceCode = carrierModel.ServiceCode;
                        express.SupplierCode = carrierModel.SupplierCode;
                        express.ProductCode = carrierModel.YanWenProductCode;
                        express.PrintSortingCode = $"{express.CompanyCode}-{express.SupplierCode}-{express.ServiceCode}-{carrierModel.RuleCode}";
                        express.SettlementCompanyCode = "10";
                        express.OutWarehouse = "10";
                        //快件信息临时表保存
                        SaveTamExpressDal.Instance.Insert(express, conn, tx);
                        tx.Commit();
                    }
                    catch (Exception e)
                    {
                        tx.Rollback();
                        result.HasError = true;
                        result.ErrorMsg = "保存快件临时表出错 " + e.Message;
                        return result;
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 获取服务供应商
        /// </summary>
        /// <param name="carrierId"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public CarrierModel GetCarrier(string carrierId, int weight)
        {
            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                string sql = @"select  * from Bs_Carrier where carrierId=@carrierId";
                if (weight > 0)
                {
                    sql += " and [MinWeight] <=@weight and [MaxWeight] >=@weight";
                }
                return conn.Query<CarrierModel>(sql, new { carrierId = carrierId, weight = weight }).ToList().FirstOrDefault();
            }
        }

        public string GetPushData()
        {
            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                string sql = @"select  * from [Test]";

                return conn.Query(sql, new { }).ToList().ToJsonString();
            }
        }

        /// <summary>
        /// 获取已经合过包的单号
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public List<string> GetCombinedNumList(List<string> orders)
        {
            using (var conn = OpenConnection("Yanwen_EPC"))
            {
                string sql = @"select  TrackingNumber from EPC_CombinedPackageDetail where IsDelete =0 and  TrackingNumber  in @orders";
                return conn.Query<string>(sql, new { orders = orders }).ToList();
            }
        }
    }
}
