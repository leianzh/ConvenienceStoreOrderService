using ConvenienceStoreOrderService.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.DTOs;

namespace ConvenienceStoreOrderService.Services.Interfaces
{
    public interface IPaymentStatusService
    {
       Result< List<PaymentStatusViewModel>> GetPaymentStatusOptions();
        Result<PaymentStatusDto> GetByCode(string paymentStatusCode);
        Result<PaymentStatusDto> GetById(int paymentStatusId);

    }
}
