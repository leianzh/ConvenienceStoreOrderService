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
        
        public DateTime CreatedAt { get; set; }
        public int ShippingFee { get; set; }
        public int OrderTotal { get; set; }
        public string CancelReason { get; set; }
        public string OrderStatusCode { get; set; }
        public string OrderStatusName { get; set; }
        public string ShippingCode { get; set; }

        public int? ShipmentStatusId { get; set; }

        public string ShipmentStatusCode { get; set; }

        public string ShipmentStatusName { get; set; }

        public string TrackingNo { get; set; }
        public int? PaymentStatusId { get; set; }
        public string PaymentStatusCode { get; set; }
        public string PaymentStatusName { get; set; }
        public string PaymentMethod { get; set; }
        
        public string RefundStatusName { get; set; }
        public string RefundStatusCode { get; set; }
        public DateTime? RefundRequestedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public string RefundReason { get; set; }
        public string RecipientName { get; set; }
        public string BuyerUserName { get; set; }

        public string UserPhone { get; set; }

        public string UserEmail { get; set; }

        public DateTime? PaymentDueAt { get; set; }

        public bool? IsCaptured { get; set; }

        public string AuthCancelStatusCode { get; set; }

        public string AuthCancelMessage { get; set; }
        public string CaptureStatusCode { get; set; }
        public string RefundApiStatusCode { get; set; }

    }
}