using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceStoreOrderService.Services.Interfaces
{
    public interface IOrderService
    {
        List<OrderViewModel> GetOrders();
        Result<bool> MarkReadyToShip(int orderId);
        Result<bool> MarkShipped(int orderId);

        Result<bool> CancelOrder(int orderId,string cancelReason);
        Result<bool> MarkArrived(int orderId);
        Result<bool> MarkPickedUp(int orderId);
        Result<int> PlaceOrder(PlaceOrderDto dto);
        Result<bool> StockWhenShipped(int orderId);
        Result<bool> ReleaseReservedStock(int orderId);
        Result<bool> ShipmentReturned(int orderId,string returnReson);
        Result<int> AutoCancelUnpaidOrders();
        Result<bool> CancelExpiredUnpaidOrder(int orderId);
        Result<int> AutoCancelIncompleteOrders();
        Result<bool> CancelExpiredIncompleteOrder(int orderId);
        Result<bool> MarkInfoCompleted(int orderId);
        Result<bool> StartPaymentCountdown(int orderId);


    }
}
