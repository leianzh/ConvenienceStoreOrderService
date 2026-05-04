using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.ViewModels
{
    public class OrderCriteria
    {
        public string SelectedPaymentStatusCode { get; set; }
        public List<PaymentStatusViewModel> PaymentStatusOptions { get; set; }
    }
}