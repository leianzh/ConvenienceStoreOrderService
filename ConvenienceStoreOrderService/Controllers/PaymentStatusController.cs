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
            var result = _paymentStatusService.GetPaymentStatusOptions();
            if(!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                criteria.PaymentStatusOptions = new List<PaymentStatusViewModel>();
                return View(criteria);
            }

            criteria.PaymentStatusOptions = result.Data;
            return View(criteria);

            
        }
    }
}