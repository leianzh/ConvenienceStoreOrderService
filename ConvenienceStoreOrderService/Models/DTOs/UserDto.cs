using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string UserPhone { get; set; }

        public string UserEmail { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}