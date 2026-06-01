using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.DTOs
{
    public class PlaceOrderDto
    {
        public int BuyerUserId { get; set; }
        public int SellerUserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int ShippingFee { get; set; }
        public string PaymentMethod { get; set; }
    }
}