using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.DTOs
{
    public class OrderStatusDto
    {
        public int OrderStatusId { get; set; }
        public string OrderStatusCode { get; set; }
        public string OrderStatusName { get; set; }
        public int OrderStatusSort { get; set; }
        public bool IsActive { get; set; }
    }
}