using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ConvenienceStoreOrderService.Models.Helpers;

namespace ConvenienceStoreOrderService.Models.EFModels
{
    public class AppDbContext : DbContext
    {


        public AppDbContext() : base(AppConfigHelper.GetDbConnectionString())
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<RefundStatus> RefundStatuses { get; set; }

    }
}