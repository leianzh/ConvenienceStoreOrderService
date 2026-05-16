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
            var dtoList= _orderRepository.GetOrders();
            return dtoList.Select(o=> OrderMapper.ToVM(o)).ToList();
        }

       public Result<bool> MarkShipped (int orderId)
        {
            var order =_orderRepository.GetEntityById(orderId);
            if(order == null)
            { return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單"); }

            // 查目前訂單狀態
            var statusIdResult = _orderStatusService.GetById(order.OrderStatusId);
            if(! statusIdResult.IsSuccess)
            {
                return Result<bool>.Fail(
            ErrorCodes.SystemError,
            "查詢目前訂單狀態失敗。");
            }

            //  查「已出貨」的資料
            var shippedStatusResult = _orderStatusService.GetByCode("Shipped");
            if(!shippedStatusResult.IsSuccess) 
            {
                return Result<bool>.Fail(
            ErrorCodes.SystemError,
            "找不到已出貨狀態設定。");
            }

            //把目前狀態 Code Id 丟給 Orders 自己判斷
            var errorMessage = order.MarkShipped(
        statusIdResult.Data.OrderStatusCode,
        shippedStatusResult.Data.OrderStatusId
    );
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
            }
            _orderRepository.SaveChanges();
            return Result<bool>.Success(true, "訂單狀態已更新為已出貨。");



        }
    }
}