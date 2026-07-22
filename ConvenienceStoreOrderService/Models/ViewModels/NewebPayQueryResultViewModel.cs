using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.ViewModels
{
    public class NewebPayQueryResultViewModel
    {
        public string Status { get; set; }
        public string Message { get; set; }

        public string MerchantOrderNo { get; set; }
        public string Amt { get; set; }
        public string TradeNo { get; set; }
        public string TradeStatus { get; set; }
        public string CloseStatus { get; set; }
        public string BackStatus { get; set; }
        public string BackBalance { get; set; }

        public string RawJson { get; set; }
    }
}