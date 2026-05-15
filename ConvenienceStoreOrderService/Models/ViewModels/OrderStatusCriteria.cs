using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.ViewModels
{
    public class OrderStatusCriteria
    {
        public string SelectedOrderStatusCode { get; set; }
       public List<OrderStatusViewModel> OrderStatusOptions { get; set; }
    }
}