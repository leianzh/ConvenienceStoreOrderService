namespace ConvenienceStoreOrderService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStatusTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderStatus",
                c => new
                    {
                        OrderStatusId = c.Int(nullable: false, identity: true),
                        OrderStatusCode = c.String(),
                        OrderStatusName = c.String(),
                        OrderStatusSort = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.OrderStatusId);
            
            CreateTable(
                "dbo.PaymentStatus",
                c => new
                    {
                        PaymentStatusId = c.Int(nullable: false, identity: true),
                        PaymentStatusCode = c.String(),
                        PaymentStatusName = c.String(),
                        PaymentStatusSort = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PaymentStatusId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PaymentStatus");
            DropTable("dbo.OrderStatus");
        }
    }
}
