using ConvenienceStoreOrderService.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceStoreOrderService.Services.Interfaces
{
    public interface IPaymentStatusService
    {
        List<PaymentStatusViewModel> GetPaymentStatusOptions();
    }
}
