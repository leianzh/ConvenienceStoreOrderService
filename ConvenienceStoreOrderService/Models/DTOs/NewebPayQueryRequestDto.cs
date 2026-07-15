using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.DTOs
{
    public class NewebPayQueryRequestDto
    {
        public string MerchantID { get; set; }
        public string Version { get; set; }
        public string RespondType { get; set; }
        public string CheckValue { get; set; }
        public string TimeStamp { get; set; }
        public string MerchantOrderNo { get; set; }
        public string Amt { get; set; }
    }
}