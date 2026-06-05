using ConvenienceStoreOrderService.Services.Interfaces;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ConvenienceStoreOrderService.Models.Common;
using Unity;

namespace ConvenienceStoreOrderService.Controllers
{
    public class OrdersController : Controller
    {
        private IOrderService _orderService;
        private IShipmentService _shipmentService;
        private IOrderDetailService _orderDetailService;
        private IPaymentService _paymentService;

        public OrdersController(IOrderService orderService, IShipmentService shipmentService, IOrderDetailService orderDetailService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _shipmentService = shipmentService;
            _orderDetailService = orderDetailService;
            _paymentService = paymentService;
        }
        // GET: Order
        public ActionResult List()
        {
            var orders = _orderService.GetOrders();
            return View(orders);
        }
        [HttpPost]
        public ActionResult MarkReadyToShip(int orderId)
        {
            var result = _orderService.MarkReadyToShip(orderId);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult MarkShipped(int orderId)
        {
            var result = _orderService.MarkShipped(orderId);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult Cancel(int orderId, string cancelReason)
        {

            var result = _orderService.CancelOrder(orderId, cancelReason);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }
            TempData["SuccessMessage"] = "訂單已取消，付款狀態已同步處理";
            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult GetShipCode(ShipmentCreateDto dto)
        {
            var result = _shipmentService.GetShipCode(dto);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }

            TempData["SuccessMessage"] = "寄件代碼產生成功：" + result.Message;

            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult UpdateShipmentAsShipped(ShipmentCreateDto dto)
        {
            var result = _shipmentService.MarkShipmentAsShipped(dto);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }

            TempData["SuccessMessage"] = "物流已寄出：" + result.Message;

            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult MarkShipmentAsArrived(ShipmentCreateDto shipmentDto)
        {
            var result = _shipmentService.MarkShipmentAsArrived(shipmentDto);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }

            TempData["SuccessMessage"] = result.Message;

            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult MarkShipmentAsPickedUp(ShipmentCreateDto shipmentDto)
        {
            var result = _shipmentService.MarkShipmentAsPickedUp(shipmentDto);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }

            TempData["SuccessMessage"] = result.Message;

            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult MarkReturned(ShipmentCreateDto shipmentDto)
        {
            var result = _shipmentService.MarkShipmentAsReturn(shipmentDto);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }

            TempData["SuccessMessage"] = result.Message;

            return RedirectToAction("List");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(PlaceOrderDto dto)
        {
            var result = _orderService.PlaceOrder(dto);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;

                // 失敗先回商品列表
                return RedirectToAction("List", "Products");
            }

            TempData["SuccessMessage"] = "下單成功";
            return RedirectToAction("List", "Orders");
        }
        public ActionResult Details(int orderId)
        {
            var result = _orderDetailService.GetOrderDetails(orderId);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List", "Orders");
            }

            return View(result.Data);

        }
        public ActionResult OrderDetailsPage(int orderId)
        {
            var result = _orderDetailService.GetOrderDetailsPage(orderId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List", "Orders");
            }

            return View(result.Data);
        }

        public ActionResult TestAutoCancelExpiredUnpaidOrders()
        {
            var result = _orderService.AutoCancelUnpaidOrders();

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = $"自動取消完成，本次取消 {result.Data} 筆訂單";
            }

            return RedirectToAction("List");
        }
        //模擬退款完成
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkRefunded(int orderId)
        {
            var result = _paymentService.MarkRefunded(
                orderId,
                "TEST_REFUND_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                "模擬藍新退款成功"
            );

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }

            TempData["SuccessMessage"] = "退款已完成。";
            return RedirectToAction("List");
        }

    }
    
}