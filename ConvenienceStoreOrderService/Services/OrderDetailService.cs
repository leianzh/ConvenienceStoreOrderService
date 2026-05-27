using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Models.DTOs;

namespace ConvenienceStoreOrderService.Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private  IOrderDetailRepository _orderDetailRepository;
        public OrderDetailService(IOrderDetailRepository orderDetailRepository) 
        {
            _orderDetailRepository = orderDetailRepository;
        }

        public Result<List<OrderDetailViewModel>> GetOrderDetails(int orderId)
        {
            var dtos = _orderDetailRepository.GetOrderDetails(orderId);
            var vm =dtos.AsEnumerable()
                .Select(o => OrderDetailMapper.ToVM(o))
                .ToList();
            return Result<List<OrderDetailViewModel>>.Success(vm);
        }
    }
}