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
        public int OrderStatusId { get;private set; }
        public int OrderSource { get; set; }
        public int PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ShippingFee { get; set; }
        public int OrderTotal { get; set; }
        public string CancelReason { get; set; }
        //處理中->待出貨
        public string MarkReadyToShip(string currentStatusCode, int shippedStatusId)
        {
            if (currentStatusCode != "Processing")
            {
                return "只有處理中的訂單可以改成待出貨。";
            }

            OrderStatusId = shippedStatusId;

            return "";
        }
        //待出貨->已出貨
        public string MarkShipped (string currentStatusCode, int shippedStatusId)
        {
            if(currentStatusCode != "ReadyToShip")
            {
                return "只有待出貨的訂單可以改成已出貨。";
            }
            OrderStatusId = shippedStatusId;
            return "";
        }
        //寄件前才能取消訂單
        public string CancelOrder(string currentStatusCode, int CancelledStatusId)
        {
            if(currentStatusCode != "Processing" && currentStatusCode != "ReadyToShip")
            {
                return "寄件前才能取消訂單";
            }
            OrderStatusId = CancelledStatusId;
            return "";
        }

    }
}