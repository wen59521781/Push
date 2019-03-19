using System.Collections.Generic;
using Yanwen.TMS.Common.Model;

namespace YanWen.EPC.Push.Common.Model
{
    public class EjfExpressModel : ModelBase
    {
        /// <summary>
        /// 快件
        /// </summary>
        public ExpressModel Express { get; set; }
        /// <summary>
        /// 报关
        /// </summary>
        public List<ExpressCustomModel> Custom { get; set; }
        public ExpressSenderModel Sender { get; set; }
        /// <summary>
        /// 收货人
        /// </summary>
        public ExpressReceiverModel Receiver { get; set; }
        /// <summary>
        /// 服务验证
        /// </summary>
        public ServiceToVerifyFieldModel ServiceVerify { get; set; }
        /// <summary>
        /// 产品验证
        /// </summary>
        public ProductToVerifyFieldModel ProductVerify { get; set; }
        /// <summary>
        /// 进门重量
        /// </summary>
        public int ReceiveWeight { get; set; }
        /// <summary>
        /// E键发换单重发老单号
        /// </summary>
        public string OldWaybillNumber { get; set; }
        /// <summary>
        /// 用于判断该快件是否需要计泡
        /// 0：不需要填长宽高，直接调保存方法  1：需要填长宽高，在调保存接口
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// 2190165 万邑通香港UPS快捷国际派送，2190021 Yodel标准POD派送 
        /// 如果这两个服务中的邮编符合验证偏远附加费的规则，则将界面中是否需要偏远附加费的复选框的背景色标红
        /// </summary>
        public bool IsRemoteSurcharge { get; set; } = false;


        #region 用于线下产品走速卖通首公里云打印用

        /// <summary>
        /// 是否含电
        /// </summary>
        public bool HasBattery { get; set; }
        /// <summary>
        /// 0.不打印退件标识和地址 1.打印丢弃标识 2.打印退回标识
        /// </summary>
        public int IsNeedReturn { get; set; }
        public string CloudPrintData { get; set; }
        #endregion
    }
}