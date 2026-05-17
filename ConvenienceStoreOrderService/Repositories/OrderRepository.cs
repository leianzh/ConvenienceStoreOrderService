using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Mappings;

namespace ConvenienceStoreOrderService.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;
        public OrderRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<OrderDto> GetOrders() 
        {
            var result =
        from o in _db.Orders
        join s in _db.OrderStatuses
            on o.OrderStatusId equals s.OrderStatusId
        select new 
        {
            Order = o,
            OrderStatusName = s.OrderStatusName
        };

            return result
                .AsEnumerable()
                .Select(o => OrderMapper.ToDto(o.Order,o.OrderStatusName))
                .ToList();

        }
        public Order GetEntityById(int orderId)
        {
            return _db.Orders
                .FirstOrDefault(o => o.OrderId == orderId);
        }
        public void SaveChanges()
        {
            _db.SaveChanges();
        }
    }
}