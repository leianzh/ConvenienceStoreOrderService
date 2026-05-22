using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.DTOs
{
    public class ShipmentCreateDto
    {
        public int ShipmentId { get; set; }
        public int OrderId { get; set; }
        //public int ShippingMethod { get; set; }
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string PickupStore { get; set; }
        public string SenderName { get; set; }
        public string SenderPhone { get; set; }
        public string ReturnStore { get; set; }
        public string TrackingNo { get; set; }
        //public int ShipmentStatus { get; set; }
        //public DateTime CreatedAt { get; set; }
        //public DateTime UpdatedAt { get; set; }
    }
}