using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }

        public T Data { get; set; }

        public string ErrorCode { get; set; }

        public string Message { get; set; }
        //成功時回傳
        public static Result<T> Success(T data, string message = "")
        {
            return new Result<T>
            {
                IsSuccess = true,
                Data = data,
                ErrorCode = null,
                Message = message
            };
        }
        //失敗時回傳
        public static Result<T> Fail(string errorCode, string message)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Data = default(T),
                ErrorCode = errorCode,
                Message = message
            };
        }
    }
}