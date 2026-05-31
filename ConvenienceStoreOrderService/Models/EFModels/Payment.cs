using Microsoft.Ajax.Utilities;
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
        public string PaymentMethod { get; set; }
        //建立訂單一開始就是Pending
        public void InitPending(int pendingStatusId)
        {
            PaymentStatusId=pendingStatusId;
        }
        //Pending取消
        public string CancelPending(int paymentStatusId)
        {
            if (PaymentStatusId != 1)
            {
               return "只有待付款可以取消";
            }

            PaymentStatusId = 4;

            return "";
        }
        //Paid取消，先維持paid
        public string CancelPaid(int paymentStatusId)
        {
            if (PaymentStatusId != 2) 
            {
                return "取消付款失敗";
            }
            PaymentStatusId = 2;
            return "";
        }
        //Paid線上
        public string MarkPaid()
        {
            if (PaymentStatusId == 2)
            {
                return "此訂單已付款，不能重複付款";
            }

            if (PaymentStatusId == 3)
            {
                return "付款失敗的紀錄不能直接改成已付款";
            }

            if (PaymentStatusId == 4)
            {
                return "已取消的付款不能改成已付款";
            }

            if (PaymentStatusId != 1)
            {
                return "只有待付款狀態可以改成已付款";
            }

            PaymentStatusId = 2;
            PaidAt = DateTime.Now;

            return "";
        }
        //Paid取付
        public string MarkPaidForCodPickedUp()
        {
            // 已付款不能重複付款
            if (PaymentStatusId == 2)
            {
                return "此 COD 訂單已付款，不能重複付款";
            }

            // 付款失敗不能直接改已付款
            if (PaymentStatusId == 3)
            {
                return "付款失敗的紀錄不能直接改成已付款";
            }

            // 已取消不能改已付款
            if (PaymentStatusId == 4)
            {
                return "已取消的付款不能改成已付款";
            }

            // COD 取貨付款，Pending -> Paid
            if (PaymentStatusId != 1)
            {
                return "只有待付款狀態的 COD 訂單可以改成已付款";
            }

            PaymentStatusId = 2;
            PaidAt = DateTime.Now;

            return "";
        }

    }
}