using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Services.Interfaces;
using ConvenienceStoreOrderService.Models.Helpers;
using ConvenienceStoreOrderService.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace ConvenienceStoreOrderService.Services
{
    public class ShipmentService : IShipmentService
    {
        private IShipmentRepository _shipmentRepository;      
        private IOrderService _orderService;
        private AppDbContext _db;
       
        public ShipmentService(IShipmentRepository shipmentRepository,IOrderService orderService,AppDbContext db)
        {
            _shipmentRepository = shipmentRepository;           
            _orderService = orderService;
            _db = db;
            
        }
        public Result<bool> GetShipCode(int orderId)
        {

            if(orderId == null)
            {  return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");  }

            var now= DateTime.Now;
            var shippingCode = CreateShippingCode(orderId);
            //訂單有沒有shipment
            var shipment = _shipmentRepository.GetByOrderId(orderId);
            if (shipment == null)
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "請先填寫物流資料");
            }

            // ShippingCode 還存在，不可重複產生
            if (!string.IsNullOrEmpty(shipment.ShippingCode))
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "此訂單已經產生過寄件代碼");
            }
            // 第一次產生寄件代碼：ShipmentStatus = Pending
            if (shipment.ShipmentStatusId == ShipmentStatusIds.Pending)
            {
                var markReadyResult = _orderService.MarkReadyToShip(orderId);

                if (!markReadyResult.IsSuccess)
                {
                    return Result<bool>.Fail(
                        markReadyResult.ErrorCode,
                        markReadyResult.Message
                    );
                }
                shipment.ShipmentStatusId = ShipmentStatusIds.ReadyToShip;
                shipment.ShippingCode = shippingCode;
                shipment.ShippingCodeGeneratedAt = now;
                shipment.UpdatedAt = now;
                _shipmentRepository.SaveChanges();

                return Result<bool>.Success(true, shippingCode);
            }
            
            // ShippingCode 被 HANGFIRE清掉後，可以重新產生
            if (shipment.ShipmentStatusId == ShipmentStatusIds.ReadyToShip)
            {
                shipment.ShippingCode = shippingCode;
                shipment.ShippingCodeGeneratedAt = now;
                shipment.UpdatedAt = now;

                _shipmentRepository.SaveChanges();

                return Result<bool>.Success(true, shippingCode);
            }

            return Result<bool>.Fail(ErrorCodes.Validation, "目前物流無法產生寄件代碼");


        }
        //新增一筆物流資料
        public Result<bool> CreateShipmentInfo(ShipmentCreateDto shipmentDto)
        {
            if (shipmentDto == null)
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "物流資料不可為空");
            }
            if (shipmentDto.OrderId == null)
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");
            }
            //看是否已有物流資料
            var haveShipment =_shipmentRepository.GetByOrderId(shipmentDto.OrderId);
            if (haveShipment != null)
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "此訂單已經填寫過物流資料");
            }
            var now = DateTime.Now;
            var shipment = ShipmentMapper.ToEntity(shipmentDto);
            shipment.ShippingMethod = 1;
            shipment.ShipmentStatusId = ShipmentStatusHelper.ShipmentStatusIds.Pending;
            shipment.ShippingCode = null;
            shipment.ShippingCodeGeneratedAt = null;
            shipment.TrackingNo = null;
            shipment.CreatedAt = now;
            shipment.UpdatedAt = now;
            _shipmentRepository.Add(shipment);
            //物流資料已填寫完成
            var completeInfoResult = _orderService.MarkInfoCompleted(shipmentDto.OrderId);
            if (!completeInfoResult.IsSuccess)
            {
                return Result<bool>.Fail(
                    completeInfoResult.ErrorCode,
                    completeInfoResult.Message
                );
            }
            
            _shipmentRepository.SaveChanges();
            return Result<bool>.Success(true, "建立物流資料成功");
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
            var shipment = _shipmentRepository.GetByOrderId(shipmentDto.OrderId);
            shipment.ShipmentStatusId = ShipmentStatusIds.Shipped;
            if (shipment.ShipmentStatusId != ShipmentStatusIds.Shipped)
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "物流狀態設定失敗，未成功設定為已寄件");
            }
            shipment.TrackingNo = trackingNo;
            shipment.UpdatedAt = now;
            _shipmentRepository.SaveChanges();
            return Result<bool>.Success(true, trackingNo);

        }
        //模擬已到店
        public Result<bool> MarkShipmentAsArrived(ShipmentCreateDto shipmentDto)
        {
            if (shipmentDto == null)
            { return Result<bool>.Fail(ErrorCodes.Validation, "物流資料不可為空"); }
            if (shipmentDto.OrderId == null)
            { { return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單"); } }


            //更新 Orders.OrderStatusId = Arrived
            var shippedResult = _orderService.MarkArrived(shipmentDto.OrderId);
            if (!shippedResult.IsSuccess)
            {
                return Result<bool>.Fail(shippedResult.ErrorCode, shippedResult.Message);
            }
            //更新 ShipmentStatus、UpdatedAt
            var now = DateTime.Now;           
            var shipment = _shipmentRepository.GetByOrderId(shipmentDto.OrderId);
            shipment.ShipmentStatusId = ShipmentStatusIds.Arrived;
            if (shipment.ShipmentStatusId != ShipmentStatusIds.Arrived)
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "物流狀態設定失敗，未成功設定為已到店");
            }
            shipment.UpdatedAt = now;
            _shipmentRepository.SaveChanges();
            return Result<bool>.Success(true, "物流更新為已到店");

        }
        //模擬已取貨
        public Result<bool> MarkShipmentAsPickedUp(ShipmentCreateDto shipmentDto)
        {
            if (shipmentDto == null)
            { return Result<bool>.Fail(ErrorCodes.Validation, "物流資料不可為空"); }
            if (shipmentDto.OrderId == null)
            { { return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單"); } }


            //更新 Orders.OrderStatusId = PickedUp
            var shippedResult = _orderService.MarkPickedUp(shipmentDto.OrderId);
            if (!shippedResult.IsSuccess)
            {
                return Result<bool>.Fail(shippedResult.ErrorCode, shippedResult.Message);
            }
            //更新 ShipmentStatus、UpdatedAt
            var now = DateTime.Now;
            var shipment = _shipmentRepository.GetByOrderId(shipmentDto.OrderId);
            shipment.ShipmentStatusId =ShipmentStatusIds.PickedUp;
            if (shipment.ShipmentStatusId != ShipmentStatusIds.PickedUp)
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "物流狀態設定失敗，未成功設定為已取貨");
            }
            shipment.UpdatedAt = now;
            _shipmentRepository.SaveChanges();
            return Result<bool>.Success(true, "物流更新為已取貨");

        }
        //模擬退貨
        public Result<bool> MarkShipmentAsReturn(ShipmentCreateDto shipmentDto)
        {
            if (shipmentDto == null)
            { return Result<bool>.Fail(ErrorCodes.Validation, "物流資料不可為空"); }
            if (shipmentDto.OrderId == null)
            { { return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單"); } }


            //更新 Orders.OrderStatusId = Return、Payment.RefundReason、Shipment=Return
            var shippedResult = _orderService.ShipmentReturned(shipmentDto.OrderId,shipmentDto.RefundReason);
            if (!shippedResult.IsSuccess)
            {
                return Result<bool>.Fail(shippedResult.ErrorCode, shippedResult.Message);
            }
            return Result<bool>.Success(true, "物流已退回，訂單已退回，已建立退款申請");

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
        //清除單筆shippingCode
        public Result<bool> ClearShippingCode(int shipmentId)
        {
            var tran=_db.Database.BeginTransaction();
            try
            {
                var shipment =_shipmentRepository.GetEntityById(shipmentId);
                if (shipment == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "找不到物流資料"); 
                }
                shipment.ClearShippingCode();
                _shipmentRepository.SaveChanges();
                tran.Commit();
                return Result<bool>.Success(true);
            }
            catch (Exception ex) 
            { 
                tran.Rollback();
                return Result<bool>.Fail(ErrorCodes.SystemError, ex.Message);
            }
            finally { tran.Dispose(); }
        }
        //清除過期shippingCode
        public Result<int> ClearExpiredShippingCodes()
        {
            var now= DateTime.Now;
            var expiredShipmentIds = _shipmentRepository.GetExpiredShippingCodeShipmentIds(now);
            int count = 0;
            List<string> errors = new List<string>();
            foreach (var shipmentId in expiredShipmentIds)
            {
                var result = ClearShippingCode(shipmentId);
                if (result.IsSuccess)
                { count++; }
                else
                {
                    errors.Add($"ShipmentId {shipmentId}：{result.Message}");
                }
            }
            if (errors.Any())
            {
                return Result<int>.Fail(
                    ErrorCodes.Validation,
                    "有寄件代碼清除失敗：" + string.Join("；", errors)
                );
            }
            return Result<int>.Success(count);
        }



    }
}