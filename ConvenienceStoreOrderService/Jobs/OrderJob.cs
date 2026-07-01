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
        //逾時未付款訂單
        public void AutoCancelExpiredUnpaidOrders()
        {
            _orderService.AutoCancelUnpaidOrders();
        }
        //物流資料逾時未填完訂單
        public void AutoCancelExpiredIncompleteOrders()
        {
            _orderService.AutoCancelIncompleteOrders();
        }
    }
}