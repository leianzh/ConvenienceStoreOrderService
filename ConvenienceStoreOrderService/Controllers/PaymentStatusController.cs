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
    public class PaymentStatusController : Controller
    {
        private IPaymentStatusService _paymentStatusService;
        public PaymentStatusController (IPaymentStatusService paymentStatusService)
        {
            _paymentStatusService = paymentStatusService;
        }
        //public JsonResult GetPaymentStatusOptions()
        //{
        //    var result = _paymentStatusService.GetPaymentStatusOptions();
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult List()
        {
            var criteria = new OrderCriteria();

            criteria.PaymentStatusOptions = _paymentStatusService.GetPaymentStatusOptions();

            return View(criteria);
        }
    }
}