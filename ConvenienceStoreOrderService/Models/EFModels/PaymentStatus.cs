using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.EFModels
{
    public class PaymentStatus
    {
        public int PaymentStatusId { get; set; }
        public string PaymentStatusCode { get; set; }
        public string PaymentStatusName { get; set; }
        public int PaymentStatusSort { get; set; }
        public bool IsActive { get; set; }
    }
}