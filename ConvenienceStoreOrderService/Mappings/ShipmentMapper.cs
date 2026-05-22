using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.Helpers;

namespace ConvenienceStoreOrderService.Mappings
{
    public static class ShipmentMapper
    {
        public static Shipment ToEntity(ShipmentCreateDto dto)
        {
            return new Shipment
            {
                OrderId = dto.OrderId,
                RecipientName = dto.RecipientName,
                RecipientPhone = dto.RecipientPhone,
                PickupStore = dto.PickupStore,
                SenderName = dto.SenderName,
                SenderPhone = dto.SenderPhone,
                ReturnStore = dto.ReturnStore,
                TrackingNo = dto.TrackingNo,
                ShipmentId = dto.ShipmentId,
                
            };
        }
        public static ShipmentCreateDto ToDto(Shipment entity)
        {
            return new ShipmentCreateDto
            {
                ShipmentId = entity.ShipmentId,
                OrderId = entity.OrderId,
                ShipmentStatusId = entity.ShipmentStatusId,
                //ShippingCode = entity.ShippingCode,
                //ShippingMethod = entity.ShippingMethod,
                //ShippingCodeGeneratedAt = entity.ShippingCodeGeneratedAt,
                TrackingNo = entity.TrackingNo,
                SenderName = entity.SenderName,
                SenderPhone = entity.SenderPhone,
                RecipientName = entity.RecipientName,
                RecipientPhone = entity.RecipientPhone,
                PickupStore = entity.PickupStore,
                ReturnStore = entity.ReturnStore,
                //CreatedAt = entity.CreatedAt,
                //UpdatedAt = entity.UpdatedAt,
                ShipmentStatusCode= ShipmentStatusHelper.GetCode(entity.ShipmentStatusId),
                ShipmentStatusName = ShipmentStatusHelper.GetName(entity.ShipmentStatusId),
            };
        }
        public static ShipmentViewModel ToVM(ShipmentCreateDto dto)
        {
            return new ShipmentViewModel
            {
                ShipmentId = dto.ShipmentId,
                OrderId = dto.OrderId,
                ShipmentStatusId = dto.ShipmentStatusId,
                //ShippingCode = dto.ShippingCode,
                //ShippingMethod = dto.ShippingMethod,
                //ShippingCodeGeneratedAt = dto.ShippingCodeGeneratedAt,
                TrackingNo = dto.TrackingNo,
                SenderName = dto.SenderName,
                SenderPhone = dto.SenderPhone,
                RecipientName = dto.RecipientName,
                RecipientPhone = dto.RecipientPhone,
                PickupStore = dto.PickupStore,
                ReturnStore = dto.ReturnStore,
                //CreatedAt = dto.CreatedAt,
                //UpdatedAt = dto.UpdatedAt,
                ShipmentStatusCode=dto.ShipmentStatusCode,
                ShipmentStatusName=dto.ShipmentStatusName,
            };
        }
    }
}