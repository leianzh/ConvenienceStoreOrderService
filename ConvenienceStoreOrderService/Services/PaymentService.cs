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
    }
}