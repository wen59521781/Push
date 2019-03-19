using System.Collections.Generic;
using Yanwen.TMS.Common.Model;

namespace YanWen.EPC.Push.Common.Model
{
    public class EubSmtWishModel : ModelBase
    {
        /// <summary>
        /// 快件
        /// </summary>
        public ExpressModel Express { get; set; }
        /// <summary>
        /// 报关
        /// </summary>
        public List<ExpressCustomModel> Custom { get; set; }
        public ExpressReceiverModel Receiver { get; set; }

        public ServiceToVerifyFieldModel Verify { get; set; }
        /// <summary>
        /// 产品验证
        /// </summary>
        public ProductToVerifyFieldModel ProductVerify { get; set; }
        /// <summary>
        /// 附加重量
        /// </summary>
        public ExpressAdditionalWeightModel ExpressAdditionalWeight { get; set; }
        /// <summary>
        /// 进门重量
        /// </summary>
        public int ReceiveWeight { get; set; }
    }
}
