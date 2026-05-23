using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Mappings;

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
        public bool ExistsByOrderId(int orderId)
        { return _db.Shipments.Any(s => s.OrderId == orderId); }
    }
}