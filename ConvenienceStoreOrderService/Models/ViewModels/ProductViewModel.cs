using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.ViewModels
{
    public class ProductViewModel
    {
        
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int Price { get; set; }
        
    }
}