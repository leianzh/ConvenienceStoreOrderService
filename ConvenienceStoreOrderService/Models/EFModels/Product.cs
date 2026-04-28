using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.EFModels
{
    public class Product
    {
        
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Price { get; set; }
        public string PDescription { get; set; }
    }
}