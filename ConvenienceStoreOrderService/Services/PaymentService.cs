using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.Constants;


namespace ConvenienceStoreOrderService.Services
{
    public class PaymentService :IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly IPaymentStatusRepository _paymentStatusRepository;
        private readonly IRefundStatusRepository _refundStatusRepository;
        public PaymentService(IPaymentRepository paymentRepository,IPaymentStatusService paymentStatusService,IPaymentStatusRepository paymentStatusRepository,IRefundStatusRepository refundStatusRepository)
        {
            _paymentRepository = paymentRepository;
            _paymentStatusService = paymentStatusService;
            _paymentStatusRepository = paymentStatusRepository;
            _refundStatusRepository = refundStatusRepository;
        }
        //根據付款狀態判斷「取消未付款」或「申請退款」
        public Result<bool> HandleCancelPayment(int orderId,string cancleReson)
        {
            var payment=_paymentRepository.GetOrderId(orderId);
            if (payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }
            //pending待付款直接取消            
            if(payment.PaymentStatusId == PaymentStatusIds.Pending)
            {
                 var errorMessage = payment.CancelPending();

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return Result<bool>.Fail(ErrorCodes.Validation, errorMessage);
                }

                return Result<bool>.Success(true);
            }
            //Paid已付款，付款狀態維持 Paid，改成退款申請中
            if (payment.PaymentStatusId == PaymentStatusIds.Paid)
            {
                var errorMessage = payment.RequestRefund(
            RefundStatusIds.Requested,
            payment.Amount,
            cancleReson
        );
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return Result<bool>.Fail(ErrorCodes.Validation, errorMessage);
                }

                return Result<bool>.Success(true);
            }
            return Result<bool>.Fail(ErrorCodes.Validation, "此付款狀態不能取消或退款");

        }
        //檢查能不能出貨
        public Result<bool> CheckCanShip(int orderId)
        {
            var payment = _paymentRepository.GetOrderId(orderId);
            if(payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }
            //COD
            if(payment.PaymentMethod == PaymentMethodName.COD)
            {
                return Result< bool>.Success(true);
            }
            //線上付款必須 Paid 才能出貨
            if(payment.PaymentStatusId ==2)
            {
                return Result<bool>.Success(true);
            }
            else
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "線上付款尚未完成，不能出貨");
            }
            
        }
        //線上付款成功
        public Result<bool> MarkPaid(int orderId)
        {                
            var payment =_paymentRepository.GetOrderId(orderId);
            if(payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");

            }
            
            var result = payment.MarkPaid();
            if(!string.IsNullOrEmpty(result))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, result);
            }
            _paymentRepository.SaveChanges();
            return Result<bool>.Success(true, "付款成功");
        }
        //COD取貨付款成功
        public Result<bool> MarkCodPaidWhenPickedUp(int orderId)
        {
            var payment = _paymentRepository.GetOrderId(orderId);

            if (payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }

            // 如果不是 COD，就不用處理
            if (payment.PaymentMethod != PaymentMethodName.COD)
            {
                return Result<bool>.Success(true);
            }

            var errorMessage = payment.MarkPaidForCodPickedUp();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
            }

            return Result<bool>.Success(true, "COD 取貨付款成功");
        }
        //申請退款
        public Result<bool> RequestRefund(int orderId, string reason)
        {
            var payment = _paymentRepository.GetOrderId(orderId);

            if (payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }

            var paymentStatus = _paymentStatusService.GetById(payment.PaymentStatusId);

            if (!paymentStatus.IsSuccess)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "查詢付款狀態失敗");
            }

            if (paymentStatus.Data.PaymentStatusCode != "Paid")
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "只有已付款訂單才能申請退款");
            }

            var refundStatus = _refundStatusRepository.GetByCode("Requested");

            if (refundStatus == null)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "找不到退款狀態：Requested");
            }

            payment.RequestRefund(
                refundStatus.RefundStatusId,
                payment.Amount,
                reason
            );

            _paymentRepository.SaveChanges();

            return Result<bool>.Success(true);
        }
        //退款完成
        public Result<bool> MarkRefunded(int orderId, string refundProviderTradeNo, string rawResponse)
        {
            var payment =_paymentRepository.GetOrderId(orderId);
            if(payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }
            // 找有沒有Requested
            var requestedStatus = _refundStatusRepository.GetByCode
                ("Requested");
            if (requestedStatus == null)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "找不到退款狀態：Requested");
            }
            //找有沒有Refunded 
            var refundedStatus = _refundStatusRepository.GetByCode("Refunded");
            if(refundedStatus == null)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "找不到退款狀態：Refunded");
            }
            var errorMessage = payment.MarkRefunded(
                requestedStatus.RefundStatusId,
                refundedStatus.RefundStatusId,
                refundProviderTradeNo,
                rawResponse
                );
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
            }

            _paymentRepository.SaveChanges();

            return Result<bool>.Success(true, "退款已完成");
        }
    }
}