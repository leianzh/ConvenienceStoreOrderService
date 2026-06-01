using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Services;
using ConvenienceStoreOrderService.Services.Interfaces;

namespace ConvenienceStoreOrderService.Jobs
{
    public class ShipmentJob
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentJob(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        public void ClearExpiredShippingCodes()
        {
            _shipmentService.ClearExpiredShippingCodes();
        }
    }
}