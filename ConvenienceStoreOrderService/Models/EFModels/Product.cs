using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.Common;

namespace ConvenienceStoreOrderService.Models.EFModels
{
    public class Product
    {
        
        public int ProductId { get; set; }
        public int SellerUserId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }

        public int TemperatureType { get; set; }
        public int StockOnHand { get; set; }
        public int StockReserved { get; set; }
        [ConcurrencyCheck]
        public int ProductVersion { get; set; }

    }
}