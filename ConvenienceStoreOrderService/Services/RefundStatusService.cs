using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.Constants;
using System.Data.Entity.Infrastructure;
using ConvenienceStoreOrderService.Services.Interfaces;
using ConvenienceStoreOrderService.Repositories.Interfaces;

namespace ConvenienceStoreOrderService.Services
{
    public class RefundStatusService :IRefundStatusService
    {
        private readonly IRefundStatusRepository _refundStatusRepository;
        public RefundStatusService(IRefundStatusRepository refundStatusRepository)
        {
            _refundStatusRepository = refundStatusRepository;
        }
    }
}