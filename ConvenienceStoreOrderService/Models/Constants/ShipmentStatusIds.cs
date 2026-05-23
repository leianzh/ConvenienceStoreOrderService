using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.Constants
{
    public class ShipmentStatusIds
    {
        public const int Pending = 1;
        public const int ReadyToShip = 2;
        public const int Shipped = 3;
        public const int Arrived = 4;
        public const int PickedUp = 5;
        public const int Cancelled = 6;
        public const int Returned = 7;
        
    }
}