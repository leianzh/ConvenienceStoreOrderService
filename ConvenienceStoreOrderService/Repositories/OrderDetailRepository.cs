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

        public OrderDetailsPageDto GetOrderDetailsPage(int orderId)
        {
            var paymentResult =
                from py in _db.Payments
                join ps in _db.PaymentStatuses
                on py.PaymentStatusId equals ps.PaymentStatusId
                select new
                {
                    OrderId=py.OrderId,
                    PaymentStatusId = py.PaymentStatusId,
                    PaymentStatusName = ps.PaymentStatusName,

                };

            var orderData =
        (from o in _db.Orders
         join os in _db.OrderStatuses
            on o.OrderStatusId equals os.OrderStatusId

         join sh in _db.Shipments
            on o.OrderId equals sh.OrderId into shipmentGroup
         from shipment in shipmentGroup.DefaultIfEmpty()

         join ps in paymentResult
            on o.OrderId equals ps.OrderId into paymentGroup
         from payment in paymentGroup.DefaultIfEmpty()

         where o.OrderId == orderId

         select new
         {
             Order = o,
             OrderStatusName = os.OrderStatusName,
             PaymentStatusName = payment == null ? "" : payment.PaymentStatusName,
             ShippingCode = shipment == null ? "" : shipment.ShippingCode ,
             TrackingNo = shipment == null ? "" :shipment.TrackingNo,
             ShipmentStatusId = shipment != null ? (int?)shipment.ShipmentStatusId : null,

         })
        .FirstOrDefault();
            if (orderData == null)
            {
                return null;
            }

            var items = _db.OrderDetails
                .Where(od => od.OrderId == orderId)
                .AsEnumerable()
                .Select(od => OrderDetailMapper.ToDto(od))
                .ToList();

            return  OrderDetailsPageMapper.ToDto(
                orderData.Order,
                orderData.OrderStatusName,
                orderData.PaymentStatusName,
                orderData.ShippingCode,
                orderData.TrackingNo,
                items,
                orderData.ShipmentStatusId
                               
            );
        }
    }
}