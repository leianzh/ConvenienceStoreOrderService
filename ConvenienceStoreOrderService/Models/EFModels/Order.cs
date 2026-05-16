using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.EFModels
{
    public class Order
    {
        public int OrderId { get; set; }
        public string OrderNo { get; set; }
        public int BuyerUserId { get; set; }
        public int SellerUserId { get; set; }
        public int OrderStatusId { get; set; }
        public int OrderSource { get; set; }
        public int PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ShippingFee { get; set; }
        public int OrderTotal { get; set; }
        public string CancelReason { get; set; }

        public string MarkShipped(string currentStatusCode, int shippedStatusId)
        {
            if (currentStatusCode != "Processing")
            {
                return "只有處理中的訂單可以改成已出貨。";
            }

            OrderStatusId = shippedStatusId;

            return "";
        }

    }
}