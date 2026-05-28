using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Services.Interfaces
{
    public interface IOrderDetailService
    {
        Result<List<OrderDetailViewModel>> GetOrderDetails(int orderId);
        Result<OrderDetailsPageViewModel> GetOrderDetailsPage(int orderId);
    }
}
