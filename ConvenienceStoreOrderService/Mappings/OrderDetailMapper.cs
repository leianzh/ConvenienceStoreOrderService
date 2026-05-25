using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.Helpers;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Mappings
{
    public class OrderDetailMapper
    {
        public static OrderDetailDto ToDto(OrderDetail entity)
        {
            return new OrderDetailDto
            {
                OrderDetailId = entity.OrderDetailId,
                OrderId = entity.OrderId,
                ProductId = entity.ProductId,
                ProductName = entity.ProductName,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                SubTotal = entity.SubTotal,
            };
        }

        public static OrderDetailViewModel ToVM(OrderDetailDto dto)
        {
            return new OrderDetailViewModel
            {
                OrderDetailId = dto.OrderDetailId,
                OrderId = dto.OrderId,
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                SubTotal = dto.SubTotal,
            };
        }
    }
}