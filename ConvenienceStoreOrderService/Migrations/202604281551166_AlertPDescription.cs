namespace ConvenienceStoreOrderService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlertPDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "PDescription", c => c.String());
            DropColumn("dbo.Products", "Description");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "Description", c => c.String());
            DropColumn("dbo.Products", "PDescription");
        }
    }
}
