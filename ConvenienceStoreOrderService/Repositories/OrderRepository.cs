using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;

namespace ConvenienceStoreOrderService.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;
        public OrderRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<OrderViewModel> GetOrders() 
        {
            return _db.Orders
                .Select(o => new OrderViewModel
                {
                    OrderNo = o.OrderNo,
                    OrderSource = o.OrderSource,
                    PaymentMethod =o.PaymentMethod,
                    CreatedAt =o.CreatedAt,
                    ShippingFee =o.ShippingFee,
                    OrderTotal =o.OrderTotal,
                    CancelReason =o.CancelReason,
                 }).ToList();
        }
    }
}