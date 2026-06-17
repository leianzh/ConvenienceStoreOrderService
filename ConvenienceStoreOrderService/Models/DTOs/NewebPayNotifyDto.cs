using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ConvenienceStoreOrderService.Models.DTOs
{
    public class NewebPayNotifyDto
    {
        
        
            public string Status { get; set; }

            public string Message { get; set; }

            public NewebPayNotifyResultDto Result { get; set; }
        

        public class NewebPayNotifyResultDto
        {
            public string MerchantID { get; set; }

            public int Amt { get; set; }

            public string TradeNo { get; set; }

            public string MerchantOrderNo { get; set; }

            public string PaymentType { get; set; }

            public string RespondCode { get; set; }

            public string Auth { get; set; }

            public string Card6No { get; set; }

            public string Card4No { get; set; }

            public string PayTime { get; set; }

            public string IP { get; set; }

            public string EscrowBank { get; set; }

            public string AuthBank { get; set; }
        }
    }
}