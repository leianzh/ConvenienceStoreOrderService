using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Mappings
{
    public class OrderStatusMapper
    {
        public static OrderStatusDto ToDto(OrderStatus entity)
        {
            return new OrderStatusDto()
            {
                OrderStatusId = entity.OrderStatusId,
                OrderStatusName = entity.OrderStatusName,
                OrderStatusCode = entity.OrderStatusCode,
                OrderStatusSort = entity.OrderStatusSort,
                IsActive = entity.IsActive,
            };
        }
        public static OrderStatusViewModel ToVM(OrderStatusDto dto)
        {
            return new OrderStatusViewModel()
            {
                OrderStatusCode = dto.OrderStatusCode,
                OrderStatusName = dto.OrderStatusName,
                
            };
        }
    }
}