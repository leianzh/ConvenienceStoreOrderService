using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.ViewModels
{
    public class OrderViewModel
    {
       public int OrderId { get; set; }
        public string OrderNo { get; set; }
        public int OrderSource { get; set; }
        public int PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ShippingFee { get; set; }
        public int OrderTotal { get; set; }
        public string CancelReason { get; set; }

        public string OrderStatusName { get; set; }
        public string ShippingCode { get; set; }

        public int? ShipmentStatusId { get; set; }

        public string ShipmentStatusCode { get; set; }

        public string ShipmentStatusName { get; set; }

        public string TrackingNo { get; set; }

        
    }
}