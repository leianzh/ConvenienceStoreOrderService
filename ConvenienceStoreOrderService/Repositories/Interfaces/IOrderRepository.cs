using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Repositories.Interfaces
{
    public interface IOrderRepository
    {
         List<OrderViewModel> GetOrders();

    }
}
