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
    public static class OrderMapper
    {
        public static  OrderDto ToDto(Order entity, string orderStatusName, string shippingCode,
    int? shipmentStatusId,string trackingNo, int? paymentStatusId,
    string paymentStatusName,string paymentMethod,string refundStatusName,string refundStatusCode, DateTime? refundRequestedAt, DateTime? refundedAt,string refundReason,string orderStatusCode,string paymentStatusCode)
        {
            return new OrderDto
            {
                OrderId = entity.OrderId,
                OrderNo = entity.OrderNo,
                BuyerUserId = entity.BuyerUserId,
                SellerUserId = entity.SellerUserId,
                OrderStatusId = entity.OrderStatusId,
                OrderSource = entity.OrderSource,               
                CreatedAt = entity.CreatedAt,
                ShippingFee = entity.ShippingFee,
                OrderTotal = entity.OrderTotal,
                CancelReason = entity.CancelReason,
                OrderStatusName = orderStatusName,
                ShippingCode = shippingCode,
                ShipmentStatusId = shipmentStatusId,
                ShipmentStatusCode = shipmentStatusId.HasValue
            ? ShipmentStatusHelper.GetCode(shipmentStatusId.Value)
            : "",
                ShipmentStatusName = shipmentStatusId.HasValue
            ? ShipmentStatusHelper.GetName(shipmentStatusId.Value)
            : "",
                TrackingNo = trackingNo,
                PaymentStatusId = paymentStatusId,
                PaymentStatusName = paymentStatusName,
                PaymentMethod = paymentMethod,
                RefundStatusName = refundStatusName,
                RefundStatusCode = refundStatusCode,
                RefundRequestedAt = refundRequestedAt,
                RefundedAt = refundedAt,
                RefundReason=refundReason,
                OrderStatusCode=orderStatusCode,
                PaymentStatusCode=paymentStatusCode,
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
                ShippingCode = dto.ShippingCode,
                ShipmentStatusId = dto.ShipmentStatusId,
                ShipmentStatusCode = dto.ShipmentStatusCode,
                ShipmentStatusName = dto.ShipmentStatusName,
                TrackingNo=dto.TrackingNo,
                PaymentStatusId=dto.PaymentStatusId,
                PaymentStatusName = dto.PaymentStatusName,
                RefundStatusName=dto.RefundStatusName,
                RefundStatusCode=dto.RefundStatusCode,
                RefundRequestedAt=dto.RefundRequestedAt,
                RefundedAt = dto.RefundedAt,
                RefundReason=dto.RefundReason,
                OrderStatusCode=dto.OrderStatusCode,
                PaymentStatusCode=dto.PaymentStatusCode,
            };
        }
    }
}