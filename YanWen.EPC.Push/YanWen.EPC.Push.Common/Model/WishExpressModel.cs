using System.Collections.Generic;
using Yanwen.TMS.Common.Model;

namespace YanWen.EPC.Push.Common.Model
{
    public class WishExpressModel
    {
        /// <summary>
        /// 快件
        /// </summary>
        public ExpressModel Express { get; set; }
        /// <summary>
        /// 报关
        /// </summary>
        public List<ExpressCustomModel> Custom { get; set; }
        /// <summary>
        /// 收货人
        /// </summary>
        public ExpressReceiverModel Receiver { get; set; }
        /// <summary>
        /// 服务验证
        /// </summary>
        public ServiceToVerifyFieldModel ServiceVerify { get; set; }

        /// <summary>
        /// 附加重量
        /// </summary>
        public ExpressAdditionalWeightModel ExpressAdditionalWeight { get; set; }
        /// <summary>
        /// 产品验证
        /// </summary>
        public ProductToVerifyFieldModel ProductVerify { get; set; }
        public ExpressToCustomerModel EXCustomer { get; set; }
        /// <summary>
        /// 燕文线下客户号
        /// </summary>
        public int ywCustomerCode { get; set; }
    }
}
