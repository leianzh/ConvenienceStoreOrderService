using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.ViewModels
{
    public class NewebPayCancelAuthorizationResultViewModel
    {
        public string RawResponse { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public string MerchantID { get; set; }

        public string MerchantOrderNo { get; set; }

        public string TradeNo { get; set; }

        public int Amt { get; set; }

        public string CheckCode { get; set; }
    }
}