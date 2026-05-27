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

namespace ConvenienceStoreOrderService.Controllers
{
    public class OrdersController : Controller
    {
        private IOrderService _orderService;
        private  IShipmentService _shipmentService;

        public OrdersController(IOrderService orderService, IShipmentService shipmentService)
        {
            _orderService = orderService;
            _shipmentService = shipmentService;
        }
        // GET: Order
        public ActionResult List()
        {
            var orders=_orderService.GetOrders();
            return View(orders);
        }
        [HttpPost]
        public ActionResult MarkReadyToShip(int orderId)
        {
            var result = _orderService.MarkReadyToShip(orderId);
            if(! result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult MarkShipped (int orderId)
        {
            var result =_orderService.MarkShipped(orderId);
            if(! result.IsSuccess) 
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }
            TempData["SuccessMessage"] =result.Message;
            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult Cancel (int orderId,string cancelReason) 
        {
            
            var result = _orderService.CancelOrder(orderId, cancelReason);
            if(! result.IsSuccess) 
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List");
            }
            TempData["SuccessMessage"] = "訂單已成功取消";
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

            TempData["SuccessMessage"] =  result.Message;

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
            return RedirectToAction("List", "Products");
        }
        public ActionResult Details(int id)
        {
            
            return View();
        }
    }
}