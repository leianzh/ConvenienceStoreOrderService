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
using Hangfire.Server;
using ConvenienceStoreOrderService.Repositories.Interfaces;

namespace ConvenienceStoreOrderService.Controllers
{
    public class OrdersController : Controller
    {
        private IOrderService _orderService;
        private IShipmentService _shipmentService;
        private IOrderDetailService _orderDetailService;
        private IPaymentService _paymentService;
        private IOrderRepository _orderRepository;
        private IUsersService _usersService;

        public OrdersController(IOrderService orderService, IShipmentService shipmentService, IOrderDetailService orderDetailService, IPaymentService paymentService, IOrderRepository orderRepository, IUsersService usersService)
        {
            _orderService = orderService;
            _shipmentService = shipmentService;
            _orderDetailService = orderDetailService;
            _paymentService = paymentService;
            _orderRepository = orderRepository;
            _usersService = usersService;
        }
        // GET: Order
        public ActionResult List(OrderSearchCriteria criteria)
        {
            
            //var orders = _orderService.GetOrders();
            var model = _orderService.GetOrderListPage(criteria);
  
            return View(model);
        }
        //模擬待出貨
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
        //模擬已出貨
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
        //取消訂單
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
        //取得寄件代碼
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
        //物流更新為已寄出
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
        //模擬已到店
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
        //模擬已取貨
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
        //下單
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
            // 將 orderId 暫存在伺服器端 Session
            Session["ShipmentOrderId"] = orderId;
            return RedirectToAction(
                "FillShipmentInfo",
                "Orders"
            );
        }
        //物流資料頁
        [HttpGet]
        public ActionResult FillShipmentInfo()
        {
            var orderId = Session["ShipmentOrderId"] as int?;
            if (!orderId.HasValue)
            {
                TempData["ErrorMessage"] = "找不到要填寫物流資料的訂單。";
                return RedirectToAction("List", "Orders");
            }
            var order =_orderRepository.GetEntityById(orderId.Value);
            var buyerResult = _usersService.GetUsers(order.BuyerUserId);
            var sellerResult = _usersService.GetUsers(order.SellerUserId);
            var dto = new ShipmentCreateDto
            {
                OrderId = orderId.Value,
                 InfoDueAt = order.InfoDueAt
            };
            if (buyerResult.IsSuccess && buyerResult.Data != null)
            {
                dto.RecipientName = buyerResult.Data.UserName;
                dto.RecipientPhone = buyerResult.Data.UserPhone;
            }

            if (sellerResult.IsSuccess && sellerResult.Data != null)
            {
                dto.SenderName = sellerResult.Data.UserName;
                dto.SenderPhone = sellerResult.Data.UserPhone;
            }

          
            dto.ReturnStore = "板橋三民門市";
            return View(dto);
        }
        //物流資料頁
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FillShipmentInfo(ShipmentCreateDto dto)
        {
            var orderId = Session["ShipmentOrderId"] as int?;
            if (!orderId.HasValue)
            {
                TempData["ErrorMessage"] = "訂單資料已失效，請重新操作。";
                return RedirectToAction("List", "Orders");
            }
            // 以伺服器 Session 中的 orderId 為準
            dto.OrderId = orderId.Value;
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
                // 使用 Session 把 OrderId 傳給 PaymentsController
                Session["PaymentOrderId"] = dto.OrderId;
                // 物流資料已完成，清除物流流程使用的 Session
                Session.Remove("ShipmentOrderId");

                return RedirectToAction(
                    "PayByCreditCard",
                    "Payments"                  
                );
            }

            //COD 填完物流資料後，回訂單列表
            Session.Remove("ShipmentOrderId");
            
            TempData["SuccessMessage"] = "物流資料已完成，下單成功。";
            return RedirectToAction("List", "Orders");
        }
        //信用卡重試交易
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RetryCreditCardPayment(int orderId)
        {
            var result = _paymentService.CreateCreditCardOnceMpgRequest(orderId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List", "Orders");
            }

            Session["PaymentOrderId"] = orderId;

            return RedirectToAction("PayByCreditCard", "Payments");
        }
        //訂單明細頁
        public ActionResult OrderDetailsPage(string orderNo)
        {
            var orderNoResult = _orderDetailService.GetOrderDetailsPageByOrderNo(orderNo);

            if (!orderNoResult.IsSuccess)
            {
                TempData["ErrorMessage"] = orderNoResult.Message;
                return RedirectToAction("List", "Orders");
            }
            int orderId = orderNoResult.Data.OrderId;
            var result = _orderDetailService.GetOrderDetailsPage(orderId);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }
            return View(result.Data);
        }

        
        //模擬cod人工退款完成
        [HttpPost]
        
        public ActionResult MarkRefunded(int orderId)
        {
            var result = _paymentService.CompleteCODRefund(orderId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("List");
        }
        

    }
    
}