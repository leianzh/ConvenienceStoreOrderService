namespace ConvenienceStoreOrderService.Migrations
{
    using ConvenienceStoreOrderService.Models.EFModels;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ConvenienceStoreOrderService.Models.EFModels.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ConvenienceStoreOrderService.Models.EFModels.AppDbContext context)
        {
            context.PaymentStatuses.AddOrUpdate(
        x => x.PaymentStatusCode,
        new PaymentStatus { PaymentStatusCode = "Pending", PaymentStatusName = "待付款", PaymentStatusSort = 1, IsActive = true },
        new PaymentStatus { PaymentStatusCode = "Paid", PaymentStatusName = "已付款", PaymentStatusSort = 2, IsActive = true },
        new PaymentStatus { PaymentStatusCode = "Failed", PaymentStatusName = "付款失敗", PaymentStatusSort = 3, IsActive = true },
        new PaymentStatus { PaymentStatusCode = "Cancelled", PaymentStatusName = "已取消", PaymentStatusSort = 4, IsActive = true }
    );
            context.OrderStatuses.AddOrUpdate(
        x => x.OrderStatusCode,
        new OrderStatus { OrderStatusCode = "Processing", OrderStatusName = "處理中", OrderStatusSort = 1, IsActive = true },
        new OrderStatus { OrderStatusCode = "Shipped", OrderStatusName = "已出貨", OrderStatusSort = 2, IsActive = true },
        new OrderStatus { OrderStatusCode = "Arrived", OrderStatusName = "已到店", OrderStatusSort = 3, IsActive = true },
        new OrderStatus { OrderStatusCode = "PickedUp", OrderStatusName = "已取貨", OrderStatusSort = 4, IsActive = true },
        new OrderStatus { OrderStatusCode = "Cancelled", OrderStatusName = "已取消", OrderStatusSort = 5, IsActive = true }
    );
        }
    }
}
