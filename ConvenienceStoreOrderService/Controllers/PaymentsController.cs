using ConvenienceStoreOrderService.Services;
using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConvenienceStoreOrderService.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET: Payments
        public ActionResult List()
        {
            return View();
        }
        [HttpGet]
        //前往信用卡付款
        public ActionResult PayByCreditCard(int orderId)
        {
            var result = _paymentService.CreateCreditCardOnceMpgRequest(orderId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List", "Orders");
            }

            return View("NewebPayPost", result.Data);
        }
        //接 ReturnURL
        [HttpPost]
        public ActionResult NewebPayReturn(string Status, string MerchantID, string Version, string TradeInfo, string TradeSha)
        {
            

            if (string.IsNullOrWhiteSpace(TradeInfo))
            {
                TempData["ErrorMessage"] = "付款完成返回失敗：沒有收到 TradeInfo。";
                return RedirectToAction("List", "Orders");
            }

            TempData["SuccessMessage"] = "付款完成，已成功返回商店頁面。";

            return RedirectToAction("List", "Orders");
        }
        [HttpPost]
        //接 NotifyURL
        public ActionResult NewebPayNotify()
        {
            var tradeInfo = Request.Form["TradeInfo"];
            var tradeSha = Request.Form["TradeSha"];

            
            
            var result = _paymentService.HandleNewebPayNotify(tradeInfo, tradeSha);

            if (!result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine("藍新 Notify 處理失敗：" + result.Message);

                
                return Content("0：" + result.Message);
                //return Content("FAIL："+result.Message);
            }

            // 藍新 Notify 成功建議回 1
            return Content("1");
            //return Content("OK");
        }
    }
}