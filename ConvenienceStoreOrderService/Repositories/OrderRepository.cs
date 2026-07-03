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
using ConvenienceStoreOrderService.Models.Helpers;

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
        public List<OrderDto> GetOrderListForDisplay(OrderSearchCriteria criteria) 
        {
            if (criteria == null)
            {
                criteria = new OrderSearchCriteria();
            }
            var paymentResult =
                from py in _db.Payments
                join ps in _db.PaymentStatuses
                on py.PaymentStatusId equals ps.PaymentStatusId

                join rs in _db.RefundStatuses
                on py.RefundStatusId equals rs.RefundStatusId
                select new 
                {
                    Payment = py,
                    PaymentStatusName = ps.PaymentStatusName,
                    PaymentMethod=py.PaymentMethod,
                    rs.RefundStatusName,
                    rs.RefundStatusCode,
                    py.RefundRequestedAt,
                    py.RefundedAt,
                    py.RefundReason,
                    ps.PaymentStatusCode,
                    
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
            s.OrderStatusCode,
            OrderStatusName = s.OrderStatusName,
            ShippingCode = shipment != null ? shipment.ShippingCode : null,
            ShipmentStatusId = shipment != null ? (int?)shipment.ShipmentStatusId : null,
            TrackingNo =shipment.TrackingNo,
            PaymentStatusId = payment.Payment.PaymentStatusId,
            PaymentStatusName =  payment.PaymentStatusName,
            PaymentMethod = payment.PaymentMethod,
            payment.RefundStatusName,
            payment.RefundStatusCode,           
            payment.RefundRequestedAt,
            payment.RefundedAt,
            payment.RefundReason,
            payment.PaymentStatusCode,
            

        };
            // 訂單編號、物流編號搜尋
            if(!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var keyword = criteria.Keyword.Trim();

                if (criteria.SearchType == "OrderNo")
                {
                    result = result.Where(x => x.Order.OrderNo.Contains(criteria.Keyword));
                }
                else if (criteria.SearchType == "TrackingNo")
                {
                    result = result.Where(x =>
                        x.TrackingNo != null &&
                        x.TrackingNo.Contains(criteria.Keyword));
                }
                else
                {
                    // SearchType 沒選時，同時查訂單編號 + 物流編號
                    result = result.Where(x =>
                        x.Order.OrderNo.Contains(keyword) ||
                        (
                            x.TrackingNo != null &&
                            x.TrackingNo.Contains(keyword)
                        ));
                }
            }
            // 訂單狀態
            if (!string.IsNullOrWhiteSpace(criteria.OrderStatusCode))
            {
                result = result.Where(x => x.OrderStatusCode == criteria.OrderStatusCode);
            }

            // 付款狀態
            if (!string.IsNullOrWhiteSpace(criteria.PaymentStatusCode))
            {
                result = result.Where(x => x.PaymentStatusCode == criteria.PaymentStatusCode);
            }
            //退款狀態
            if(!string.IsNullOrWhiteSpace(criteria.RefundStatusCode))
            {
                result = result.Where(x =>x.RefundStatusCode == criteria.RefundStatusCode);
            }
            //物流狀態
            if (criteria.ShipmentStatusId.HasValue)
            {
                result= result.Where(x =>
                x.ShipmentStatusId.HasValue &&
                x.ShipmentStatusId.Value
                == criteria.ShipmentStatusId.Value);
            }

            // 付款方式
            if (!string.IsNullOrWhiteSpace(criteria.PaymentMethod))
            {
                result = result.Where(x => x.PaymentMethod == criteria.PaymentMethod);
            }

            // 開始日期
            if (criteria.StartDate.HasValue)
            {
                result = result.Where(x => x.Order.CreatedAt >= criteria.StartDate.Value);
            }

            // 結束日期
            if (criteria.EndDate.HasValue)
            {
                var endDateExclusive = criteria.EndDate.Value.AddDays(1);
                result = result.Where(x => x.Order.CreatedAt < endDateExclusive);
            }

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
                    o.PaymentMethod,
                    o.RefundStatusName,
                    o.RefundStatusCode,
                    o.RefundRequestedAt,
                    o.RefundedAt,
                    o.RefundReason,
                    o.OrderStatusCode,
                    o.PaymentStatusCode
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
        //查詢逾期訂單
       public List<int> GetUnpaidOrderIds(DateTime now)
        {
            //Orders、Payments、PaymentStatuses、OrderStatuses
            var result =
                from o in _db.Orders
                join p in _db.Payments
                on o.OrderId equals p.OrderId
                join ps in _db.PaymentStatuses
                on p.PaymentStatusId equals ps.PaymentStatusId
                join os in _db.OrderStatuses
                on o.OrderStatusId equals os.OrderStatusId
                where o.PaymentDueAt != null
                && o.PaymentDueAt < now
                && ps.PaymentStatusCode == "Pending"
                && os.OrderStatusCode == "Processing"
                && p.PaymentMethod != "COD"
                select o.OrderId;

            return result.ToList();


        }
        //查物流資料填寫時間
        public List<int> GetIncompleteOrderIds(DateTime now)
        {
            var result =
                from o in _db.Orders
                join os in _db.OrderStatuses
                on o.OrderStatusId equals os.OrderStatusId
                where o.InfoDueAt != null
                && o.InfoDueAt < now
                && o.InfoCompletedAt == null
                && os.OrderStatusCode == "Processing"
                select o.OrderId;
            return result.ToList();
        }

    }
}