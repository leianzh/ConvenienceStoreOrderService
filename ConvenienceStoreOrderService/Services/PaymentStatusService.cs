using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Services
{
    public class PaymentStatusService : IPaymentStatusService
    {
         private readonly IPaymentStatusRepository _paymentStatusRepository;

    public PaymentStatusService(IPaymentStatusRepository paymentStatusRepository)
    {
        _paymentStatusRepository = paymentStatusRepository;
    }

    public List<PaymentStatusViewModel> GetPaymentStatusOptions()
    {
        return _paymentStatusRepository.GetPaymentStatusOptions();
    }

}
}
