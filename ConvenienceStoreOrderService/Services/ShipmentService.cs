using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Services
{
    public class ShipmentService : IShipmentService
    {
        private IShipmentRepository _shipmentRepository;      
        private IOrderService _orderService;
       
        public ShipmentService(IShipmentRepository shipmentRepository,IOrderService orderService )
        {
            _shipmentRepository = shipmentRepository;           
            _orderService = orderService;
            
        }
        public Result<bool> GetShipCode(ShipmentCreateDto shipmentDto)
        {
            
            if (shipmentDto == null)
            { return Result<bool>.Fail(ErrorCodes.Validation, "物流資料不可為空"); }
            if(shipmentDto.OrderId == null)
            { { return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單"); } }

            if (_shipmentRepository.ExistsByOrderId(shipmentDto.OrderId))
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "此訂單已經產生過寄件代碼");
            }
            
            var markReadyResult = _orderService.MarkReadyToShip(shipmentDto.OrderId);

            if (!markReadyResult.IsSuccess)
            {
                return Result<bool>.Fail(markReadyResult.ErrorCode, markReadyResult.Message);
            }
            var now = DateTime.Now;
            var shippingCode = CreateShippingCode(shipmentDto.OrderId);
            var shipment =ShipmentMapper.ToEntity(shipmentDto);
            shipment.ShippingMethod = 1;
            shipment.ShipmentStatusId = 2;
            shipment.ShippingCode = shippingCode;
            shipment.ShippingCodeGeneratedAt = now;
            shipment.CreatedAt = now;
            shipment.UpdatedAt = now;

            _shipmentRepository.Add(shipment);
            _shipmentRepository.SaveChanges();

            return Result<bool>.Success(true,shippingCode);


        }
        //模擬已寄件
        public Result<bool> MarkShipmentAsShipped(ShipmentCreateDto shipmentDto)
        {
            if (shipmentDto == null)
            { return Result<bool>.Fail(ErrorCodes.Validation, "物流資料不可為空"); }
            if (shipmentDto.OrderId == null)
            { { return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單"); } }
            

            //更新 Orders.OrderStatusId = Shipped
            var shippedResult = _orderService.MarkShipped(shipmentDto.OrderId);
            if (!shippedResult.IsSuccess)
            {
                return Result<bool>.Fail(shippedResult.ErrorCode, shippedResult.Message);
            }
            //更新 ShipmentStatus、TrackingNo
            var now = DateTime.Now;
            var trackingNo = CreateTrackingNo(shipmentDto.OrderId);
            var shipment = _shipmentRepository.UpdateShipmentAsShipped(shipmentDto.OrderId);
            shipment.ShipmentStatusId = 3;
            shipment.TrackingNo = trackingNo;
            shipment.UpdatedAt = now;
            _shipmentRepository.SaveChanges();
            return Result<bool>.Success(true, trackingNo);

        }
        //產生寄件代碼
        public string CreateShippingCode(int orderId)
        {
            return $"SHIP{DateTime.Now:yyyyMMdd}{orderId.ToString().PadLeft(4, '0')}";
        }
        //產生物流追蹤碼
        public string CreateTrackingNo(int orderId)
        {
            return "T"
                + DateTime.Now.ToString("yyyyMMddHHmmss")
                + new Random().Next(1000, 9999);
        }
    }
}