using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.DTOs
{
    public class NewebPayCloseRequestDto
    {
        public string MerchantID { get; set; }
        public string PostData { get; set;}

    }
}