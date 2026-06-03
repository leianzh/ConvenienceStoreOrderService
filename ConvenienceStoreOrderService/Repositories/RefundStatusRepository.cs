using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Mappings;
using Microsoft.Ajax.Utilities;
using static Unity.Storage.RegistrationSet;

namespace ConvenienceStoreOrderService.Repositories
{
    public class RefundStatusRepository : IRefundStatusRepository
    {
        private readonly AppDbContext _db;
        public RefundStatusRepository(AppDbContext db) 
        {  _db = db; 
        }

        public RefundStatusDto GetByCode(string refundStatusCode)
        {
            var entity = _db.RefundStatuses.FirstOrDefault(r => r.RefundStatusCode == refundStatusCode && r.IsActive);
            if (entity == null) 
            {
                return null;
            }
            return RefundStatusMapper.ToDto(entity);
        }
    }
}