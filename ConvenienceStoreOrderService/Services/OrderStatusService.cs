using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.DTOs;
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

        public Result<OrderStatusDto> GetByCode(string orderStatusCode)
        {
            var dto=_orderStatusRepository.GetByCode(orderStatusCode);
            if(dto == null)
            {
                return Result<OrderStatusDto>.Fail(ErrorCodes.Validation, "找不到訂單狀態");
            }
            return Result<OrderStatusDto>.Success(dto);
        }

        public Result<OrderStatusDto> GetById(int orderStatusId)
        {
            var dto =_orderStatusRepository.GetById(orderStatusId);
            if(dto == null)
            {
                return Result<OrderStatusDto>.Fail(ErrorCodes.Validation, "找不到訂單");
            }
            return Result<OrderStatusDto>.Success(dto);
        }

        public List<OrderStatusViewModel> GetOrderStatusesOptions()
        {
            var dtoList= _orderStatusRepository.GetOrderStatusesOptions();
            return dtoList.Select(o => OrderStatusMapper.ToVM(o)).ToList();
        }

    }
}