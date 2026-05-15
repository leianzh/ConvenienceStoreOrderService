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
    public class OrderStatusRepository : IOrderStatusRepository
    {
        private AppDbContext _db;
        public OrderStatusRepository(AppDbContext db) 
        {
            _db = db;
        }
        
        public List<OrderStatusDto> GetOrderStatusesOptions()
        {
            return _db.OrderStatuses
                .Where(o=>o.IsActive)
                .OrderBy(o=>o.OrderStatusSort)
                .AsEnumerable()
                .Select(o=> OrderStatusMapper.ToDto(o))
                .ToList();
        }
    }
}