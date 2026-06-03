using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.ViewModels
{
    public class RefundStatusViewModel
    {
        public int RefundStatusId { get; set; }
        public string RefundStatusCode { get; set; }
        public string RefundStatusName { get; set; }
        public int RefundStatusSort { get; set; }
        public bool IsActive { get; set; }
    }
}