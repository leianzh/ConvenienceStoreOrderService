using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.Common
{
    public static class ErrorCodes
    {
        public const string Validation = "Validation";
        public const string NotFound = "NotFound";
        public const string Conflict = "Conflict";
        public const string SystemError = "SystemError";
    }
}