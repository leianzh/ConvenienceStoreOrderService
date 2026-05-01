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

        public List<PaymentStatusViewModel> GetPaymentStatusOptions()
        {
            return _db.PaymentStatuses
         .Where(x => x.IsActive)
         .OrderBy(x => x.PaymentStatusSort)
         .Select(x => new PaymentStatusViewModel
         {
             PaymentStatusCode = x.PaymentStatusCode,
             PaymentStatusName = x.PaymentStatusName
         })
         .ToList();
        }
    }
}