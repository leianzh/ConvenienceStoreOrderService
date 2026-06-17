using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Repositories.Interfaces;

namespace ConvenienceStoreOrderService.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _db;
        public PaymentRepository(AppDbContext db)
        {
            _db = db;
        }

        public void Add(Payment payment)
        {
            _db.Payments.Add(payment);
        }

        public Payment GetOrderId(int orderId)
        {
            return _db.Payments.FirstOrDefault(x => x.OrderId == orderId);
        }
        public void SaveChanges()
        {
            _db.SaveChanges();
        }
        public Payment GetByOrderNo(string orderNo)
        {
            return
                (from p in _db.Payments
                 join o in _db.Orders
                    on p.OrderId equals o.OrderId
                 where o.OrderNo == orderNo
                 select p)
                .FirstOrDefault();
        }
    }
}