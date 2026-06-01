using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;

namespace ConvenienceStoreOrderService.Repositories.Interfaces
{
    public interface IShipmentRepository
    {
        List<ShipmentCreateDto> GetShipCode();
        void SaveChanges();
        void Add(Shipment shipment);
        bool ExistsByOrderId(int orderId);
        Shipment UpdateShipment(int orderId);
        List<int> GetExpiredShippingCodeShipmentIds(DateTime now);
        Shipment GetEntityById(int shipmentId);
    }
}
