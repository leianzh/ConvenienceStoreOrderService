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
    }
}