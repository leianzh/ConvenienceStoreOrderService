using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Services;
using ConvenienceStoreOrderService.Services.Interfaces;

namespace ConvenienceStoreOrderService.Jobs
{
    public class OrderJob
    {
        private readonly IOrderService _orderService;

        public OrderJob(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public void AutoCancelExpiredUnpaidOrders()
        {
            _orderService.AutoCancelUnpaidOrders();
        }
    }
}