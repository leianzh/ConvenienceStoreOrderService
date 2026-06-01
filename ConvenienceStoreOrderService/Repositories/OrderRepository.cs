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
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentStatusRepository _paymentStatusRepository;
        public OrderRepository(AppDbContext db,IPaymentRepository paymentRepository,    IPaymentStatusRepository paymentStatusRepository)
        {
            _db = db;
            _paymentRepository = paymentRepository;
            _paymentStatusRepository = paymentStatusRepository;
        }
        
        
        //查訂單列表 + 查訂單狀態名稱 + 查物流資料+付款狀態
        public List<OrderDto> GetOrderListForDisplay() 
        {
            var paymentResult =
                from py in _db.Payments
                join ps in _db.PaymentStatuses
                on py.PaymentStatusId equals ps.PaymentStatusId
                select new 
                {
                    Payment = py,
                    PaymentStatusName = ps.PaymentStatusName,
                    PaymentMethod=py.PaymentMethod,
                };

            var result =
        from o in _db.Orders
        join s in _db.OrderStatuses
            on o.OrderStatusId equals s.OrderStatusId

        join sh in _db.Shipments
            on o.OrderId equals sh.OrderId into shipmentGroup
            from shipment in shipmentGroup.DefaultIfEmpty()

            join ps in paymentResult
            on o.OrderId equals ps.Payment.OrderId into paymentGroup
            from payment in paymentGroup.DefaultIfEmpty()


        select new 
        {
            Order = o,
            OrderStatusName = s.OrderStatusName,
            ShippingCode = shipment != null ? shipment.ShippingCode : null,
            ShipmentStatusId = shipment != null ? (int?)shipment.ShipmentStatusId : null,
            TrackingNo =shipment.TrackingNo,
            PaymentStatusId = payment.Payment.PaymentStatusId,
            PaymentStatusName =  payment.PaymentStatusName,
            PaymentMethod = payment.PaymentMethod

        };

            return result
                .AsEnumerable()
                .Select(o => OrderMapper.ToDto(
                    o.Order,
                    o.OrderStatusName,
                    o.ShippingCode,
                    o.ShipmentStatusId,
                    o.TrackingNo,
                    o.PaymentStatusId,
                    o.PaymentStatusName,
                    o.PaymentMethod
                    ))
                .ToList();

            
        }

        public Order GetEntityById(int orderId)
        {
            return _db.Orders
                .FirstOrDefault(o => o.OrderId == orderId);
        }
        //新增一筆訂單
        public void Add(Order order)
        {
            _db.Orders.Add(order);
        }
        public void SaveChanges()
        {
            _db.SaveChanges();
        }

        public List< OrderDetail> GetOrderDetailId(int orderId)
        {
            return _db.OrderDetails.Where(od => od.OrderId == orderId).ToList();
        }
    }
}