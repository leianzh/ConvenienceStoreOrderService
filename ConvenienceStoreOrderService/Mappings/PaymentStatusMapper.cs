using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Mappings
{
    public static class PaymentStatusMapper
    {
        public static PaymentStatusDto ToDto(PaymentStatus entity)
        {
            return new PaymentStatusDto
            {
                PaymentStatusId = entity.PaymentStatusId,
                PaymentStatusCode = entity.PaymentStatusCode,
                PaymentStatusName = entity.PaymentStatusName,
                PaymentStatusSort = entity.PaymentStatusSort,
                IsActive = entity.IsActive,
            };
        }
        public static PaymentStatusViewModel ToVm(PaymentStatusDto dto)
        {
            return new PaymentStatusViewModel
            {
                PaymentStatusId = dto.PaymentStatusId,
                PaymentStatusCode = dto.PaymentStatusCode,
                PaymentStatusName = dto.PaymentStatusName,
                PaymentStatusSort = dto.PaymentStatusSort,
                IsActive = dto.IsActive,
            };
        }
    }
}