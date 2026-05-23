using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Services;
using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConvenienceStoreOrderService.Controllers
{
    public class OrderstatusController : Controller
    {
        private IOrderStatusService _orderStatusService;
        public OrderstatusController(IOrderStatusService orderStatusService)
        {
            _orderStatusService = orderStatusService;
        }
        // GET: Orderstatus
        public ActionResult List()
        {
            var criteria = new OrderStatusCriteria();
            criteria.OrderStatusOptions = _orderStatusService.GetOrderStatusesOptions();
            return View(criteria);
        }
    }
}