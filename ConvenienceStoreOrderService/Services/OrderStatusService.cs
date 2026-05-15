using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Services
{
    public class OrderStatusService : IOrderStatusService
    {
        private IOrderStatusRepository _orderStatusRepository;
        public OrderStatusService (IOrderStatusRepository orderStatusRepository)
        {
            _orderStatusRepository = orderStatusRepository;
        }
        public List<OrderStatusViewModel> GetOrderStatusesOptions()
        {
            var dtoList= _orderStatusRepository.GetOrderStatusesOptions();
            return dtoList.Select(o => OrderStatusMapper.ToVM(o)).ToList();
        }
    }
}