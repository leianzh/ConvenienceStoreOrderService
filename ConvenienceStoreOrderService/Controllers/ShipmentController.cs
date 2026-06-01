using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConvenienceStoreOrderService.Controllers
{
    public class ShipmentController : Controller
    {
        private IShipmentService _shipmentService;
        public ShipmentController (IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }
        // GET: Shipment
        public ActionResult List()
        {
            return View();
        }
        public ActionResult TestClearExpiredShippingCodes()
        {
            var result = _shipmentService.ClearExpiredShippingCodes();

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = $"清除過期寄件代碼完成，本次清除 {result.Data} 筆";
            }

            return RedirectToAction("List", "Orders");
        }
    }
}