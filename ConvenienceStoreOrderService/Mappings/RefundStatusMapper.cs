using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.Helpers;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Mappings
{
    public class RefundStatusMapper
    {
        public static RefundStatusDto ToDto(RefundStatus entity)
        {
            return new RefundStatusDto
            {
                RefundStatusId = entity.RefundStatusId,
                RefundStatusCode = entity.RefundStatusCode,
                RefundStatusName = entity.RefundStatusName,
                RefundStatusSort = entity.RefundStatusSort,
                IsActive = entity.IsActive,
            };
        }
        public static RefundStatusViewModel ToVM(RefundStatusDto dto) 
        {
            return new RefundStatusViewModel
            {
                RefundStatusId = dto.RefundStatusId,
                RefundStatusCode = dto.RefundStatusCode,
                RefundStatusName = dto.RefundStatusName,
                RefundStatusSort = dto.RefundStatusSort,
                IsActive = dto.IsActive,
            };
        }
    }
}