using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Common;

namespace ConvenienceStoreOrderService.Repositories
{
    public class OrderStatusRepository : IOrderStatusRepository
    {
        private AppDbContext _db;
        public OrderStatusRepository(AppDbContext db) 
        {
            _db = db;
        }

        public OrderStatusDto GetByCode(string orderStatusCode)
        {
            var entity=_db.OrderStatuses
                .FirstOrDefault(o => o.OrderStatusCode == orderStatusCode && o.IsActive);
            if (entity == null) 
            {
                return null;
            }
            return OrderStatusMapper.ToDto(entity);
        }

        public OrderStatusDto GetById(int orderStatusId)
        {
            var entity =_db.OrderStatuses
                .FirstOrDefault(o =>o.OrderStatusId == orderStatusId && o.IsActive);
            if (entity == null) { return null; }
            return OrderStatusMapper.ToDto(entity);
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