using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Yanwen.TMS.Base;
using Yanwen.TMS.Base.DataAccess;
using Yanwen.TMS.Common.Cache;
using Yanwen.TMS.Common.DataAccess;
using Yanwen.TMS.Common.Logic;
using Yanwen.TMS.Common.Model;
using YanWen.EPC.Push.Common.Model;

namespace YanWen.EPC.Push.Common.DataAccess
{
    public class SaveTamExpressDal : BaseDal
    {

        public static readonly Lazy<SaveTamExpressDal> Lazy = new Lazy<SaveTamExpressDal>(() => new SaveTamExpressDal());
        public static SaveTamExpressDal Instance => Lazy.Value;

        public Result<bool> InsertExpressReceiverCustom(Result<WishExpressModel> model)
        {
            var result = new Result<bool> { HasError = false };

            using (var conn = OpenConnection())
            {
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        //快件信息临时表保存
                        Insert(model.Data.Express, conn, tx);

                        //验证单号是否存在报关中
                        var custom = new ExpressCustomDal();
                        if (!custom.ExistExpressCustom(model.Data.Express.WaybillNumber, conn, tx))
                        {
                            //快件报关保存
                            custom.Insert(model.Data.Custom, conn, tx);
                        }
                        var receiver = new ExpressReceiverDal();
                        if (!receiver.Exist(model.Data.Express.WaybillNumber, conn, tx))
                        {
                            //快件收货人保存
                            receiver.Insert(model.Data.Receiver, conn, tx);
                        }                     
                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        result.HasError = true;
                        result.ErrorMsg = "保存失败" + ex.Message;
                    }
                }
            }
            return result;
        }


        public int Insert(ExpressModel model, IDbConnection conn, IDbTransaction tx)
        {
            var sql = "Insert into [dbo].[EPC_TemExpress] (StatusId ,WaybillNumber,ExchangeNumber,OrderCode,CustomerCode, " +
                      "ServiceCode,SupplierCode, RegionId, ReceiveDate, TrueWeight, ExpressLength, ExpressWidth, " +
                      "ExpressHigh,CompanyCode, Piece, Memo, CreateId, CreateTime, SourceId, ArDeclaredValue, " +
                      "ApDeclaredValue,DeclaredCurrencyId, SettlementCompanyCode,YanwenNumber,PrintSortingCode, " +
                      "NeedRemoteSurcharge,NeedOverLength,CustomerDiscountLevel,ProductCode,PostCode, " +
                      "IsArSpecial,IsApSpecial,ArSpecialAmount,ApSpecialAmount,AreaCode,EPReceiveDate,OutWarehouse, " +
                      "DestinationWarehouse,SupplierWeight,[SalesCode],PlatformOrderTime,WarehouseSurcharge) " +
                      "values(@StatusId,@WaybillNumber,@ExchangeNumber,@OrderCode,@CustomerCode,@ServiceCode, " +
                      "@SupplierCode,@RegionId,@ReceiveDate,@TrueWeight,@ExpressLength,@ExpressWidth,@ExpressHigh, " +
                      "@CompanyCode,@Piece,@Memo,@CreateId,@CreateTime,@SourceId,@ArDeclaredValue,@ApDeclaredValue, " +
                      "@DeclaredCurrencyId,@SettlementCompanyCode,@YanwenNumber,@PrintSortingCode,@NeedRemoteSurcharge, " +
                      "@NeedOverLength,@CustomerDiscountLevel,@ProductCode,@PostCode,@IsArSpecial,@IsApSpecial, " +
                      "@ArSpecialAmount,@ApSpecialAmount,@AreaCode,@EPReceiveDate,@OutWarehouse,@DestinationWarehouse, " +
                      "@SupplierWeight,@SalesCode,@PlatformOrderTime,@WarehouseSurcharge)";            
                var count = conn.Execute(sql, new
                {
                    StatusId = model.StatusId != 10 ? 1 : model.StatusId,
                    model.WaybillNumber,
                    model.ExchangeNumber,
                    model.OrderCode,
                    model.CustomerCode,
                    model.ServiceCode,
                    model.SupplierCode,
                    model.RegionId,
                    model.ReceiveDate,
                    model.TrueWeight,
                    model.ExpressLength,
                    model.ExpressWidth,
                    model.ExpressHigh,
                    model.CompanyCode,
                    model.Piece,
                    Memo = model.Memo ?? string.Empty,
                    model.CreateId,
                    model.CreateTime,
                    model.SourceId,
                    model.ArDeclaredValue,
                    ApDeclaredValue = model.ArDeclaredValue,
                    model.DeclaredCurrencyId,
                    model.SettlementCompanyCode,
                    YanwenNumber = model.YanwenNumber ?? "",
                    model.PrintSortingCode,
                    model.NeedRemoteSurcharge,
                    model.NeedOverLength,
                    model.CustomerDiscountLevel,
                    model.ProductCode,
                    PostCode = model.PostCode.Trim(),
                    IsArSpecial = false,
                    IsApSpecial = false,
                    ArSpecialAmount = 0,
                    ApSpecialAmount = 0,
                    model.AreaCode,
                    model.EPReceiveDate,
                    OutWarehouse = model.OutWarehouse ?? "10",//默认东莞
                    DestinationWarehouse = model.DestinationWarehouse ?? "",
                    model.SupplierWeight,
                    model.SalesCode,
                    model.PlatformOrderTime,
                    model.WarehouseSurcharge,
                }, tx);
                return count;                     
        }
    }
}
