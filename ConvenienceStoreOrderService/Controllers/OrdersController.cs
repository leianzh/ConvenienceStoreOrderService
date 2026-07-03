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
using ConvenienceStoreOrderService.Models.Constants;
using ConvenienceStoreOrderService.Models.EFModels;

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
        public ActionResult List(OrderSearchCriteria criteria)
        {
            
            //var orders = _orderService.GetOrders();
            var model = _orderService.GetOrderListPage(criteria);
  
            return View(model);
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
            var result = _shipmentService.GetShipCode(dto.OrderId);

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
        //訂單退回、物流退貨，建立退款申請
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
            var orderId = result.Data;
            // 如果是信用卡一次付清，下單成功後導去藍新付款流程
            //if (dto.PaymentMethod == PaymentMethodName.CreditCard)
            //{
           
            //    return RedirectToAction(
            //        "PayByCreditCard",
            //        "Payments",
            //        new { orderId = orderId }
            //    );
            //}
            // COD 下單成功就回訂單列表
            //TempData["SuccessMessage"] = "下單成功";
            //return RedirectToAction("List", "Orders");
            // 下單成功後，先去填物流資料
            return RedirectToAction(
                "FillShipmentInfo",
                "Orders",
                new { orderId = orderId }
            );
        }
        [HttpGet]
        public ActionResult FillShipmentInfo(int orderId)
        {
            var dto = new ShipmentCreateDto
            {
                OrderId = orderId
            };

            return View(dto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FillShipmentInfo(ShipmentCreateDto dto)
        {
            // 物流資料
            var saveResult = _shipmentService.CreateShipmentInfo(dto);

            if (!saveResult.IsSuccess)
            {
                TempData["ErrorMessage"] = saveResult.Message;
                return View(dto);
            }

            // 付款方式
            var paymentMethodResult = _paymentService.GetPaymentMethodByOrderId(dto.OrderId);

            if (!paymentMethodResult.IsSuccess)
            {
                TempData["ErrorMessage"] = paymentMethodResult.Message;
                return RedirectToAction("List", "Orders");
            }

            var paymentMethod = paymentMethodResult.Data;

            // 信用卡，填完物流資料後才開始付款倒數，然後導去藍新
            if (paymentMethod == PaymentMethodName.CreditCard)
            {
                var countdownResult = _orderService.StartPaymentCountdown(dto.OrderId);

                if (!countdownResult.IsSuccess)
                {
                    TempData["ErrorMessage"] = countdownResult.Message;
                    return RedirectToAction("List", "Orders");
                }

                return RedirectToAction(
                    "PayByCreditCard",
                    "Payments",
                    new { orderId = dto.OrderId }
                );
            }

            //COD 填完物流資料後，回訂單列表
            TempData["SuccessMessage"] = "物流資料已完成，下單成功。";
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
        
        public ActionResult MarkRefunded(int orderId)
        {
            var result = _paymentService.MarkRefunded(
                orderId,
                "TEST_REFUND_" + DateTime.Now.ToString("yyyyMMddHHmm"),
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