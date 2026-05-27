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
    public class PaymentRepository :IPaymentRepository
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

        
    }
}