using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.Constants;

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
        public int RefundStatusId { get; set; }
        public DateTime? RefundRequestedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public int? RefundAmount { get; set; }
        public string RefundReason { get; set; }
        public string RefundProviderTradeNo { get; set; }
        public string RefundRawResponse { get; set; }
        //建立訂單一開始就是Pending
        public void InitPending(int pendingStatusId)
        {
            PaymentStatusId=pendingStatusId;
        }
        //Pending取消付款
        public string CancelPending()
        {
            if (PaymentStatusId != PaymentStatusIds.Pending)
            {
               return "只有待付款可以取消";
            }

            PaymentStatusId = PaymentStatusIds.Cancelled;
            UpdatedAt = DateTime.Now;

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
        //Paid 申請退款
        public string RequestRefund(int refundStatusId, int refundAmount, string refundReason)
        {
            if (PaymentStatusId != PaymentStatusIds.Paid)
            {
                return "只有已付款訂單可以申請退款";
            }

            if (RefundStatusId == RefundStatusIds.Requested)
            {
                return "此付款已申請退款，不能重複申請";
            }

            if (RefundStatusId == RefundStatusIds.Refunded)
            {
                return "此付款已退款完成，不能重複退款";
            }
            if (refundAmount <= 0)
            {
                return "退款金額必須大於 0";
            }
            if (string.IsNullOrWhiteSpace(refundReason))
            {
                return "退款原因必填";
            }

            //申請退款PaymentStatusId 不改，繼續維持 Paid
            RefundStatusId = refundStatusId;
            RefundAmount = refundAmount;
            RefundReason = refundReason;
            RefundRequestedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            return "";
        }
        //退款完成
        public void MarkRefunded(int refundedStatusId, string refundProviderTradeNo, string rawResponse)
        {
            RefundStatusId = refundedStatusId;
            RefundedAt = DateTime.Now;
            RefundProviderTradeNo = refundProviderTradeNo;
            RefundRawResponse = rawResponse;
            UpdatedAt = DateTime.Now;
        }
    }
}