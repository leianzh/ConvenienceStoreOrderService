using ConvenienceStoreOrderService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Mappings;

namespace ConvenienceStoreOrderService.Repositories
{
    public class OrderDetailRepository :IOrderDetailRepository
    {
        private readonly AppDbContext _db;
        public OrderDetailRepository (AppDbContext db)
        {
            _db = db;
        }

        

        public List<OrderDetailDto> GetOrderDetails(int orderId)
        {
            
            var orderDetail = _db.OrderDetails
                .Where(od => od.OrderId == orderId)
                .AsEnumerable()
                .Select(o =>OrderDetailMapper.ToDto(o))
                .ToList();
            return orderDetail;
            
        }
        public void Add(OrderDetail orderDetail)
        {
            _db.OrderDetails.Add(orderDetail);
        }

        
    }
}