using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yanwen.TMS.Base;
using Yanwen.TMS.Common.Cache;
using Yanwen.TMS.Common.Logic;
using YanWen.EPC.Push.Common.DataAccess;
using YanWen.EPC.Push.Common.Model;

namespace YanWen.EPC.Push.Common.Logic
{
    public class SaveExpressTemLogic
    {
        public static readonly Lazy<SaveExpressTemLogic> Lazy = new Lazy<SaveExpressTemLogic>(() => new SaveExpressTemLogic());
        public static SaveExpressTemLogic Instance => Lazy.Value;


        /// <summary>
        /// 快件保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="carrier"></param>
        /// <returns></returns>
        public Result<WishExpressModel> AddExpress(WishExpressModel model, int carrier)
        {
            var result = new Result<WishExpressModel> { HasError = false, Data = model };
            var carrierModel = EpcDal.Instance.GetCarrier(carrier.ToString(), model.Express.TrueWeight);
            if (carrierModel != null)
            {
                //替换产品号
                model.Express.ProductCode = carrierModel.YanWenProductCode;
                model.Express.ServiceCode = carrierModel.ServiceCode;
                model.Express.SupplierCode = carrierModel.SupplierCode;
            }
            //保存验证
            var varifyResult = VerifySave(new Result<WishExpressModel> { HasError = false, Data = model });
            if (varifyResult.HasError)
            {
                result.HasError = varifyResult.HasError;
                result.ErrorMsg = varifyResult.ErrorMsg;
                return result;
            }
            varifyResult.Data.Express.SupplierWeight = varifyResult.Data.Express.TrueWeight;
            //note:调取打印分拣码
            var sortingCode = ExpressOperate.Instance.GetPrintSortingCode(model.Express);
            if (sortingCode.HasError)
            {
                result.HasError = true;
                result.ErrorMsg = sortingCode.ErrorMsg;
                return result;
            }
            #region 验证中外运等产品是否计泡

            if (model.ProductVerify.IsGlobalWeight)
            {
                var resultGlobal = ExpressOperate.Instance.JudgeGlobal(model.ServiceVerify, model.Express);
                if (resultGlobal.HasError)
                {
                    result.HasError = true;
                    result.ErrorMsg = resultGlobal.ErrorMsg;
                    return result;
                }
                model.ProductVerify.IsGlobalWeight = resultGlobal.Data;
            }

            #endregion

            #region 附加重量类

            //var resultConvertAdditionalWeight = ExpressOperate.Instance.ConvertAdditionalWeight(model.Express, model.ServiceVerify, model.ProductVerify);
            //if (resultConvertAdditionalWeight.HasError)
            //{
            //    result.HasError = true;
            //    result.ErrorMsg = resultConvertAdditionalWeight.ErrorMsg;
            //    return result;
            //}
            //model.ExpressAdditionalWeight = resultConvertAdditionalWeight.Data;
            #endregion

            varifyResult.Data.Express.PrintSortingCode = sortingCode.Data;
            varifyResult.Data.Express.AreaCode = WarehouseCache.GetWarehouse(varifyResult.Data.Express.CompanyCode).AreaCode;
            result.Data = varifyResult.Data;
            //保存TMS临时表
            SaveTamExpressDal.Instance.InsertExpressReceiverCustom(varifyResult);
            result.DetailMsg = varifyResult.Data.Express.PrintSortingCode;
            return result;
        }

        /// <summary>
        /// 快件保存验证
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Result<WishExpressModel> VerifySave(Result<WishExpressModel> model)
        {
            #region 产品验证: 重量，申报价值，增值税

            var result = ProductExpressVerify.Instance.ProductVerify(model.Data.Express, model.Data.ProductVerify, 6);
            if (result.HasError)
            {
                model.HasError = result.HasError;
                model.ErrorMsg = result.ErrorMsg;
                return model;
            }

            #endregion

            model.Data.Express = model.Data.Express;
            return model;
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="model"></param>
        /// <param name="orders"></param>
        /// <param name="item"></param>
        /// <param name="exchangeNumber"></param>
        /// <returns></returns>
        public EpcCombineModel ConversionModel(EpcCombineResponseModel model, List<string> orders, CombineResponsePushModel item, string exchangeNumber)
        {
            var reslut = new EpcCombineModel { };
            reslut.ExchangeNumber = exchangeNumber;
            reslut.BarCode = model.barcode;
            reslut.CombineId = item.Number;
            reslut.Weight = EpcDal.Instance.GetEpcCombineWeight(orders, item.Parameter,item.Parameter1);
            reslut.carrier = model.carrier;
            reslut.Piece = orders.Count;
            reslut.pdf_10_en_url = model.pdf_10_en_url;
            reslut.pdf_10_lcl_url = model.pdf_10_lcl_url;
            reslut.pdf_a4_en_url = model.pdf_a4_en_url;
            reslut.pdf_a4_lcl_url = model.pdf_a4_lcl_url;
            reslut.label_format = model.label_format;
            reslut.PackageLabelsType = item.Parameter;
            return reslut;
        }
    }
}
