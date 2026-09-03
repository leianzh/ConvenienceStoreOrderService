using ConvenienceStoreOrderService.Services;
using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ConvenienceStoreOrderService.Models.ViewModels;

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
        public ActionResult PayByCreditCard()
        {
            var orderId = Session["PaymentOrderId"] as int?;
            var result = _paymentService.CreateCreditCardOnceMpgRequest(orderId.Value);
            Session.Remove("PaymentOrderId");
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QueryNewebPayTrade(int orderId)
        {
            var result = _paymentService.QueryTradeInfo(orderId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List", "Orders");
            }
            return View("NewebPayQueryResult", result.Data);
        }
        //  顯示「藍新請款 / 退款測試」頁面
        [HttpGet]
        public ActionResult CloseTrade()
        {
            return View();
        }
        //發動藍新 Close API       
        // closeType：
        // 1 = 請款 
        // 2 = 退款        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CloseTrade(int orderId, int closeType)
        {

            if (orderId <= 0)
            {
                TempData["ErrorMessage"] = "OrderId 不正確。";
                return RedirectToAction("CloseTrade");
            }

            if (closeType != 1 && closeType != 2)
            {
                TempData["ErrorMessage"] = "CloseType 只能是 1（請款）或 2（退款）。";
                return RedirectToAction("CloseTrade");
            }
            // 呼叫 PaymentService
            var result = _paymentService.CloseTrade(
                orderId,
                closeType
            );

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("CloseTrade");
            }
            
            return View(result.Data);
        }
        //退款失敗重試
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RetryRefund(int orderId)
        {
            var result = _paymentService.ProcessRefund(orderId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("List", "Orders");
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("List", "Orders");
        }
    }
}