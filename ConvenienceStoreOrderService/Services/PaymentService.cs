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
        public PaymentService(IPaymentRepository paymentRepository,IPaymentStatusService paymentStatusService,IPaymentStatusRepository paymentStatusRepository)
        {
            _paymentRepository = paymentRepository;
            _paymentStatusService = paymentStatusService;
            _paymentStatusRepository = paymentStatusRepository;
        }
        
        public Result<bool> CancelPayment(int orderId)
        {
            var payment=_paymentRepository.GetOrderId(orderId);
            if (payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }
            //pending待付款            
            if(payment.PaymentStatusId == 1)
            {
                //改成cancel
                payment.CancelPending(payment.PaymentStatusId);
            }
            //Paid已付款
            if (payment.PaymentStatusId == 2)
            {
                payment.CancelPaid(payment.PaymentStatusId);
            }
            return Result<bool>.Success(true);

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
    }
}