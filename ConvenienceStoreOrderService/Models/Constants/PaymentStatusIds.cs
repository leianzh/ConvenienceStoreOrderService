using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.Constants
{
    public class PaymentStatusIds
    {
        public const int Pending = 1;
        public const int Paid = 2;
        public const int Failed = 3;
        public const int Cancelled = 4;
    }
}