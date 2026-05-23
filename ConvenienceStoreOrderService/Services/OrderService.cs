using ConvenienceStoreOrderService.Services.Interfaces;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.EFModels;

namespace ConvenienceStoreOrderService.Services
{
    public class OrderService : IOrderService
    {
        private IOrderRepository _orderRepository;
        private IOrderStatusService _orderStatusService;
        public OrderService(IOrderRepository orderRepository, IOrderStatusService orderStatusService)
        {
            _orderRepository = orderRepository;
            _orderStatusService = orderStatusService;
        }
        public List<OrderViewModel> GetOrders()
        {
            var dtoList= _orderRepository.GetOrderListForDisplay();
            return dtoList.Select(o=> OrderMapper.ToVM(o)).ToList();
        }
        //Processing->ReadyToShip
        public Result<bool> MarkReadyToShip (int orderId)
        {
            var order =_orderRepository.GetEntityById(orderId);
            if(order == null)
            { return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單"); }

            // 查目前訂單狀態
            var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
            if(!currentStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(
            ErrorCodes.SystemError,
            "查詢目前訂單狀態失敗。");
            }

            //  查「待出貨」的資料
            var targetStatusResult = _orderStatusService.GetByCode("ReadyToShip");
            if(!targetStatusResult.IsSuccess) 
            {
                return Result<bool>.Fail(
            ErrorCodes.SystemError,
            "找不到已出貨狀態設定。");
            }

            //把目前狀態 Code Id 丟給 Orders 自己判斷
            var errorMessage = order.MarkReadyToShip(
                targetStatusResult.Data.OrderStatusId,
                currentStatusResult.Data.OrderStatusCode
        
    );
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
            }
            _orderRepository.SaveChanges();
            return Result<bool>.Success(true, "訂單狀態已更新為待出貨。");

        }
        //ReadyToShip->Shipped
        public Result<bool> MarkShipped (int orderId) 
        {
            var order =_orderRepository.GetEntityById(orderId);
            if(order == null) 
            {  return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");  }
            //查Order的OrderStatusId目前訂單狀態
            var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
            if (!currentStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(
            ErrorCodes.SystemError,
            "查詢目前訂單狀態失敗。");
            }
            //查「已出貨」
            var targetStatusResult = _orderStatusService.GetByCode("Shipped");
            if (!targetStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(
                    ErrorCodes.SystemError,
                    "找不到已出貨狀態設定。"
                );
            }
            var errorMessage = order.MarkShipped
                (targetStatusResult.Data.OrderStatusId,
                currentStatusResult.Data.OrderStatusCode
                );
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
            }
            _orderRepository.SaveChanges();
            return Result<bool>.Success(true, "訂單狀態已更新為已出貨。");
        }
        //Shipped->Arrived
        public Result<bool> MarkArrived(int orderId)
        {
            var order = _orderRepository.GetEntityById(orderId);
            if(order == null)
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");
            }
            //查Order的OrderStatusId目前訂單狀態
            var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
            if(!currentStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError,
            "查詢目前訂單狀態失敗。");
            }
            //查要改的StatusCode「已到店」狀態
            var targetStatusResult = _orderStatusService.GetByCode("Arrived");
            if (!targetStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError,
                    "找不到已到店狀態設定。");
            }
            //改orderstatusId
            var result = order.MarkArrived
                (
                 targetStatusResult.Data.OrderStatusId,
                currentStatusResult.Data.OrderStatusCode

                );
            if (!string.IsNullOrEmpty(result))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, result);
            }
            _orderRepository.SaveChanges();
            return Result<bool>.Success(true, "訂單狀態已更新為已到店。");
        }
        //Arrived->PickedUp
        public Result<bool> MarkPickedUp(int orderId)
        {
            var order = _orderRepository.GetEntityById(orderId);
            if (order == null)
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");
            }
            //查Order的OrderStatusId目前訂單狀態
            var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
            if (!currentStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError,
            "查詢目前訂單狀態失敗。");
            }
            //查要改的StatusCode「已取貨」狀態
            var targetStatusResult = _orderStatusService.GetByCode("PickedUp");
            if (!targetStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError,
                    "找不到已到店狀態設定。");
            }
            //改orderstatusId
            var result = order.MarkPickedUp
                (
                 targetStatusResult.Data.OrderStatusId,
                currentStatusResult.Data.OrderStatusCode

                );
            if (!string.IsNullOrEmpty(result))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, result);
            }
            _orderRepository.SaveChanges();
            return Result<bool>.Success(true, "訂單狀態已更新為已取貨。");
        }
        public Result<bool> CancelOrder (int orderId,string cancelReson) 
        {
            //取消原因必填
            if (string.IsNullOrWhiteSpace(cancelReson))
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "取消原因必填");
            }
            //找orderId
            var order = _orderRepository.GetEntityById(orderId);
            if (order == null)
            { return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單"); }
            //找order.statusId的code、name
            var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
if(!currentStatusResult.IsSuccess) 
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "找不到物流狀態");
            }
            //找Cancelled對應的id、name
            var targetStatusResult = _orderStatusService.GetByCode("Cancelled");
            if(!targetStatusResult.IsSuccess) 
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "找不到取消訂單");
            }
            //把code、id丟給ORDER.CS判斷，不是就回錯誤訊息
            var errorMessage = order.CancelOrder(
                targetStatusResult.Data.OrderStatusId, currentStatusResult.Data.OrderStatusCode);
            if (!string.IsNullOrEmpty(errorMessage)) 
            {
                return Result<bool>.Fail(ErrorCodes.Conflict,errorMessage);
            }
            order.CancelReason = cancelReson;
            _orderRepository.SaveChanges();
            return Result<bool>.Success(true, "訂單已取消。");
            
        }
    }
}