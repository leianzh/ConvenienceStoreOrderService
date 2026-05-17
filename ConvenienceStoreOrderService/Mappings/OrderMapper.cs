using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Mappings
{
    public static class OrderMapper
    {
        public static  OrderDto ToDto(Order entity, string orderStatusName)
        {
            return new OrderDto
            {
                OrderId = entity.OrderId,
                OrderNo = entity.OrderNo,
                BuyerUserId = entity.BuyerUserId,
                SellerUserId = entity.SellerUserId,
                OrderStatusId = entity.OrderStatusId,
                OrderSource = entity.OrderSource,
                PaymentMethod = entity.PaymentMethod,
                CreatedAt = entity.CreatedAt,
                ShippingFee = entity.ShippingFee,
                OrderTotal = entity.OrderTotal,
                CancelReason = entity.CancelReason,
                OrderStatusName = orderStatusName,
            };
        }
        public static OrderViewModel ToVM(OrderDto dto)
        {
            return new OrderViewModel 
            {
                OrderId = dto.OrderId,
                OrderNo = dto.OrderNo,
                OrderSource =dto.OrderSource,
                PaymentMethod=dto.PaymentMethod,
                CreatedAt=dto.CreatedAt,
                ShippingFee=dto.ShippingFee,
                OrderTotal=dto.OrderTotal,
                CancelReason=dto.CancelReason,
                OrderStatusName = dto.OrderStatusName,
            };
        }
    }
}