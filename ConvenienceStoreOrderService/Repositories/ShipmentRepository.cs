using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Constants;

namespace ConvenienceStoreOrderService.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private AppDbContext _db;
        public ShipmentRepository(AppDbContext db)
        {  _db = db; }

        public List<ShipmentCreateDto> GetShipCode()
        {
            var result = _db.Shipments
                .Select(s => ShipmentMapper.ToDto(s))
                .AsEnumerable() .ToList();
            return result;
                
        }
        public void SaveChanges()
        { _db.SaveChanges(); }
        public void Add(Shipment shipment)
        {
            _db.Shipments.Add(shipment);
        }
        public Shipment UpdateShipment(int orderId)
        {
            return _db.Shipments
                .FirstOrDefault(s => s.OrderId == orderId);
        }
        public Shipment GetEntityById(int shipmentId)
        {
            return _db.Shipments
                .FirstOrDefault(s => s.ShipmentId == shipmentId);
        }
        public bool ExistsByOrderId(int orderId)
        { return _db.Shipments.Any(s => s.OrderId == orderId); }
        //查過期寄件shippcode
        public List<int> GetExpiredShippingCodeShipmentIds(DateTime now)
        {
            
            var expiredTime = now.AddDays(-4);
            //Shipments、Orders、OrderStatuses
            var result =
                from sh in _db.Shipments
                join o in _db.Orders
                on sh.OrderId equals o.OrderId
                join os in _db.OrderStatuses
                on o.OrderStatusId equals os.OrderStatusId
                where sh.ShippingCode != null
                && sh.ShippingCodeGeneratedAt != null
                && sh.ShippingCodeGeneratedAt < expiredTime               
                && sh.ShipmentStatusId == ShipmentStatusIds.ReadyToShip
                && os.OrderStatusCode == "ReadyToShip"
                select sh.ShipmentId;
            return result.ToList();
        }
    }
}