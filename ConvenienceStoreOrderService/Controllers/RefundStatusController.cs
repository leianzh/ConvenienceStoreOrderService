using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Services;
using ConvenienceStoreOrderService.Services.Interfaces;

namespace ConvenienceStoreOrderService.Controllers
{
    public class RefundStatusController : Controller
    {
        private IRefundStatusService _refundStatusService;
        public RefundStatusController (IRefundStatusService refundStatusService)
        {
            _refundStatusService = refundStatusService;
        }
        // GET: RefundStatus
        public ActionResult List()
        {
            return View();
        }
    }
}