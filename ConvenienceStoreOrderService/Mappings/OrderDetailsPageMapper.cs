using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.Constants;
using ConvenienceStoreOrderService.Models.Helpers;

namespace ConvenienceStoreOrderService.Mappings
{
    public static class OrderDetailsPageMapper
    {
        public static OrderDetailsPageDto ToDto(
    Order entity,
    string orderStatusName,
    string paymentStatusName,
    string shippingCode,
    string trackingNo,
    List<OrderDetailDto> items,   
    int? shipmentStatusId)
        {
            return new OrderDetailsPageDto
            {
                OrderId = entity.OrderId,
                OrderNo = entity.OrderNo,
                CreatedAt = entity.CreatedAt,

                OrderStatusName = orderStatusName,
                //PaymentStatusName = paymentStatusName,

                ShippingFee = entity.ShippingFee,              
                OrderTotal = entity.OrderTotal,
                ShippingCode = shippingCode,               
                TrackingNo = trackingNo,
                ShipmentStatusId = shipmentStatusId,
                ShipmentStatusCode = shipmentStatusId.HasValue
            ? ShipmentStatusHelper.GetCode(shipmentStatusId.Value)
            : "",
                ShipmentStatusName = shipmentStatusId.HasValue
            ? ShipmentStatusHelper.GetName(shipmentStatusId.Value)
            : "",

                Items = items
            };
        }
        public static OrderDetailsPageViewModel ToVm(OrderDetailsPageDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new OrderDetailsPageViewModel
            {
                OrderId = dto.OrderId,
                OrderNo = dto.OrderNo,
                CreatedAt = dto.CreatedAt,

                OrderStatusName = dto.OrderStatusName,
                //PaymentStatusName = dto.PaymentStatusName,

                //PaymentMethod = dto.PaymentMethod,
                ShippingFee = dto.ShippingFee,
                OrderTotal = dto.OrderTotal,

                ShippingCode = dto.ShippingCode,               
                TrackingNo = dto.TrackingNo,
                ShipmentStatusId= dto.ShipmentStatusId,
                ShipmentStatusCode= dto.ShipmentStatusCode,
                ShipmentStatusName = dto.ShipmentStatusName,
                Items = dto.Items == null
                    ? new List<OrderDetailViewModel>()
                    : dto.Items.Select(x => OrderDetailMapper.ToVM(x)).ToList()

            };
        }

    }
}