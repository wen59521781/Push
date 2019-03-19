using System;

namespace YanWen.EPC.Push.Common.Model
{
    public class EpcExpressModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 追踪单号
        /// </summary>
        public string TrackingNumber { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        public int TrueWeight { get; set; }
        /// <summary>
        /// 接收大包号
        /// </summary>
        public string PackageNumber { get; set; }
        /// <summary>
        /// 合包ID
        /// </summary>
        public string CombineId { get; set; }
        /// <summary>
        /// OrderId
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// 单号状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 生成时间（生成跟踪单号的时间）
        /// </summary>
        public DateTime ReleasedToMerchantTime { get; set; }
        /// <summary>
        /// 价值
        /// </summary>
        public decimal Value { get; set; }
        /// <summary>
        /// 下架指令
        /// </summary>
        public bool ReadyToShip { get; set; }
        /// <summary>
        /// 推荐上架区域（X区X架X层X格）
        /// </summary>
        public string RecommendedArea { get; set; }
        /// <summary>
        /// 目的国
        /// </summary>
        public string DestinationCountry { get; set; }

        /// <summary>
        /// 实际上架区域（X区X架X层X格）
        /// </summary>
        public string ActualArea { get; set; }
        /// <summary>
        /// 上下架状态（0：录入，1：上架，2：下架）仓内实际
        /// </summary>
        public int ShelvesStatus { get; set; }
        /// <summary>
        /// 大包标签类型
        /// </summary>
        public int PackageLabelsType { get; set; }

        /// <summary>
        /// 录入时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 拣配单打印状态 0:初始状态 1:打印过
        /// </summary>
        public int PrintState { get; set; }
        /// <summary>
        /// 大包标签类型名称
        /// </summary>
        public string PackageLabelsTypeName { get; set; }
        /// <summary>
        /// 是否合包
        /// </summary>
        public bool IsOrPackage { get; set; }
        /// <summary>
        /// 是否液体
        /// </summary>
        public bool IsLiquid { get; set; }
        /// <summary>
        /// 是否含电
        /// </summary>
        public bool IsBattery { get; set; }
        /// <summary>
        /// 是否敏感
        /// </summary>
        public bool IsSensitive { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 公司ID
        /// </summary>
        public string WarehouseId { get; set; }

        /// <summary>
        /// EPC没有服务 ，用于计费，需要设计默认值
        /// </summary>
        public string ServiceCode { get; set; }

        /// <summary>
        /// EPC没有产品 ，用于计费，需要设计默认值
        /// </summary>
        public string ProductCode { get; set; }
    }
}
