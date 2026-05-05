using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.DTOs
{
    public class PaymentStatusDto
    {
        public string PaymentStatusCode { get; set; }
        public string PaymentStatusName { get; set; }
    }
}