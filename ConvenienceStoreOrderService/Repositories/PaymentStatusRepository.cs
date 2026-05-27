using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Repositories
{
    public class PaymentStatusRepository : IPaymentStatusRepository
    {
        private readonly AppDbContext _db;

        public PaymentStatusRepository(AppDbContext db)
        {
            _db = db;
        }

        public PaymentStatusDto GetByCode(string paymentStatusCode)
        {
            var entity =_db.PaymentStatuses
                .FirstOrDefault(p => p.PaymentStatusCode == paymentStatusCode && p.IsActive);
            if (entity == null) 
            {
                return null;
            }
            return PaymentStatusMapper.ToDto(entity);
        }

        public List<PaymentStatusDto> GetPaymentStatusOptions()
        {
            return _db.PaymentStatuses
         .Where(x => x.IsActive)
         .OrderBy(x => x.PaymentStatusSort)
         .AsEnumerable()
         .Select(x => PaymentStatusMapper.ToDto(x))
         .ToList();
        }
    }
}