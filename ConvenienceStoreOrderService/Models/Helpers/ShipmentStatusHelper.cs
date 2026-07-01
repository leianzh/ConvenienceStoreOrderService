using ConvenienceStoreOrderService.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.Helpers
{
    public class ShipmentStatusHelper
    {
        public class ShipmentStatusIds
        {
            public const int Pending = 1;
            public const int ReadyToShip = 2;
            public const int Shipped = 3;
            public const int Arrived = 4;
            public const int PickedUp = 5;
            public const int Returned = 6;
            public const int Cancelled = 7;
        }
        public static string GetCode(int shipmentStatusId)
        {
            switch (shipmentStatusId)
            {
                case ShipmentStatusIds.Pending:
                    return "Pending";

                case ShipmentStatusIds.ReadyToShip:
                    return "ReadyToShip";

                case ShipmentStatusIds.Shipped:
                    return "Shipped";

                case ShipmentStatusIds.Arrived:
                    return "Arrived";

                case ShipmentStatusIds.PickedUp:
                    return "PickedUp";

                case ShipmentStatusIds.Returned:
                    return "Returned";

                case ShipmentStatusIds.Cancelled:
                    return "Cancelled";

                default:
                    return "Unknown";
            }
        }

        public static string GetName(int shipmentStatusId)
        {
            switch (shipmentStatusId)
            {
                case ShipmentStatusIds.Pending:
                    return "待建立";

                case ShipmentStatusIds.ReadyToShip:
                    return "待寄件";

                case ShipmentStatusIds.Shipped:
                    return "已寄件";

                case ShipmentStatusIds.Arrived:
                    return "已到店";

                case ShipmentStatusIds.PickedUp:
                    return "已取貨";

                case ShipmentStatusIds.Returned:
                    return "已退回";

                case ShipmentStatusIds.Cancelled:
                    return "已取消";

                default:
                    return "未知狀態";
            }
        }
    }
}