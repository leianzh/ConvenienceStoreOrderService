using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.Constants
{
    public class RefundStatusIds
    {
        public const int None = 1;
        public const int Requested = 2;
        public const int Refunded = 3;
        public const int Failed = 4;
    }
}