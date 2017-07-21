﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MFS.Models
{
    public class Product : EntityBase
    {
        /// <summary>
        /// 底价
        /// </summary>
        public decimal MinPrice { get; set; }
        /// <summary>
        /// 商品分类
        /// </summary>
        [JsonIgnore]
        public virtual ProductType ProductType { get; set; }
    }

    public class ProductType : EntityBase
    {
        public ProductType()
        {
            if (Products == null)
                Products = new List<Product>();
        }
        public virtual ICollection<Product> Products { get; set; }
    }
}
