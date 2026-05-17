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
    }
}