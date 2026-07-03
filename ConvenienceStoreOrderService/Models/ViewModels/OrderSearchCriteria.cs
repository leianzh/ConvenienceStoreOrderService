using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.ViewModels
{
    public class OrderSearchCriteria
    {
        // OrderNo / TrackingNo
        public string SearchType { get; set; }

        public string Keyword { get; set; }

        public string OrderStatusCode { get; set; }

        public string PaymentStatusCode { get; set; }
        public string RefundStatusCode { get; set; }
        public int? ShipmentStatusId { get; set; }

        public string PaymentMethod { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}