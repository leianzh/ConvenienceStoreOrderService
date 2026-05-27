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
        public int OrderTotal { get; set; }//SubTotal+ShippingFee
        public string CancelReason { get; set; }
        public DateTime? PaymentDueAt { get; set; }
        //建立訂單一開始就是 Processing
        public void InitProcessing(int processingStatusId)
        {
            OrderStatusId=processingStatusId;
        }

        //處理中->待出貨
        public string MarkReadyToShip( int statusId, string currentStatusCode)
        {
            if (currentStatusCode != "Processing")
            {
                return "只有處理中的訂單可以改成待出貨。";
            }

            OrderStatusId = statusId;

            return "";
        }
        //待出貨->已出貨
        public string MarkShipped (int statusId, string currentStatusCode)
        {
            if(currentStatusCode != "ReadyToShip")
            {
                return "只有待出貨的訂單可以改成已出貨。";
            }
            OrderStatusId = statusId;
            return "";
        }
        //已出貨->已到店
        public string MarkArrived (int statusId, string currentStatusCode)
        {
            if (currentStatusCode != "Shipped")
            {
                return "只有已出貨的訂單可以改成已到店。";
            }
            OrderStatusId = statusId;
            return "";
        }
        //已到店->已取貨
        public string MarkPickedUp(int statusId, string currentStatusCode)
        {
            if (currentStatusCode != "Arrived")
            {
                return "只有已到店的訂單可以改成已取貨。";
            }
            OrderStatusId = statusId;
            return "";
        }
        //寄件前才能取消訂單
        public string CancelOrder(int statusId, string currentStatusCode)
        {
            if(currentStatusCode != "Processing" && currentStatusCode != "ReadyToShip")
            {
                return "寄件前才能取消訂單";
            }
            OrderStatusId = statusId;
            return "";
        }

    }
}