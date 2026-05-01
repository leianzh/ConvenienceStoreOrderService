using ConvenienceStoreOrderService.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceStoreOrderService.Repositories.Interfaces
{
    public interface IPaymentStatusRepository
    {
        List<PaymentStatusViewModel> GetPaymentStatusOptions();
    }
}
