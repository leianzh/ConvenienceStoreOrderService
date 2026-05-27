using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.EFModels;

namespace ConvenienceStoreOrderService.Repositories.Interfaces
{
    public interface IOrderStatusRepository
    {
        List<OrderStatusDto> GetOrderStatusesOptions();
        OrderStatusDto GetById(int orderStatusId);
        OrderStatusDto GetByCode(string orderStatusCode);

        
    }
}
