using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.EFModels
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string PaymentProvider { get; set; }
        public int PaymentStatusId { get;private set; }
        public string TradeNo { get; set; }
        public int Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public string RawCallBack { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        //建立訂單一開始就是Pending
        public void InitPending(int pendingStatusId)
        {
            PaymentStatusId=pendingStatusId;
        }
    }
}