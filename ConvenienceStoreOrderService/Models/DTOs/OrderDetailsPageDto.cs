using ConvenienceStoreOrderService.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.DTOs
{
    public class OrderDetailsPageDto
    {
        public int OrderId { get; set; }
        public string OrderNo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string OrderStatusName { get; set; }

        public string PaymentStatusName { get; set; }
        public int ShippingFee { get; set; }
        public int OrderTotal { get; set; }
        public string ShippingCode { get; set; }
        public int? ShipmentStatusId { get; set; }

        public string ShipmentStatusCode { get; set; }
        public string ShipmentStatusName { get; set; }
        public string TrackingNo { get; set; }
        public List<OrderDetailDto> Items { get; set; }
       

        

    }
}