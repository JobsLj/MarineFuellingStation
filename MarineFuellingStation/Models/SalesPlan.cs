﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MFS.Models
{
    /// <summary>
    /// 销售计划 Name字段为单号
    /// </summary>
    public class SalesPlan : EntityBase
    {
        public SalesPlanType SalesPlanType { get; set; }
        public string CarNo { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 当前余油
        /// </summary>
        public decimal Remainder { get; set; }
        public DateTime OilDate { get; set; }
        /// <summary>
        /// 是否开票
        /// </summary>
        public bool IsInvoice { get; set; }
        /// <summary>
        /// 开票单位
        /// </summary>
        public string BillingCompany { get; set; }
        /// <summary>
        /// 开票单价
        /// </summary>
        public decimal BillingPrice { get; set; }
        /// <summary>
        /// 开票数量
        /// </summary>
        public int BillingCount { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalMoney
        {
            get
            {
                return Price * Count;
            }
        }
    }

    public enum SalesPlanType
    {
        水上,
        陆上,
        机油
    }
}