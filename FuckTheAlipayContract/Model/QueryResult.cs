using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckTheAlipayContract.Model
{
    public class QueryResult
    {
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 交易号
        /// </summary>
        public string TradeNo { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public string Amount1 { get; set; }
        /// <summary>
        /// 服务费
        /// </summary>
        public string PostalFee { get; set; }
        /// <summary>
        /// 应付金额
        /// </summary>
        public string Amount2 { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateOn { get; set; }
        /// <summary>
        /// 付款时间
        /// </summary>
        public DateTime PaymentOn { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndOn { get; set; }
        public string Info { get; set; }
    }
}
