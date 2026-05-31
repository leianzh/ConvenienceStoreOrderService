using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;

namespace ConvenienceStoreOrderService.Repositories.Interfaces
{
    public interface IOrderRepository
    {
         List<OrderDto> GetOrderListForDisplay();
        Order GetEntityById(int orderId);
        void Add(Order order);

        void SaveChanges();
       List< OrderDetail> GetOrderDetailId(int orderId);
    }
}
