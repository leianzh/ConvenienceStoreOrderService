using ConvenienceStoreOrderService.Services.Interfaces;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConvenienceStoreOrderService.Controllers
{
    public class OrdersController : Controller
    {
        private IOrderService _orderService;
        public OrdersController(IOrderService orderService)
        {
            _orderService= orderService;
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
    }
}