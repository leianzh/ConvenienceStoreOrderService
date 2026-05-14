using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.DTOs;

namespace ConvenienceStoreOrderService.Repositories.Interfaces
{
    public interface IOrderRepository
    {
         List<OrderDto> GetOrders();

    }
}
