using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.ViewModels
{
    public class OrderListPageViewModel
    {
        public OrderSearchCriteria Criteria { get; set; }
        public List<OrderViewModel> Orders { get; set; }
    }
}