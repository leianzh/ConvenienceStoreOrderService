using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using ConvenienceStoreOrderService.Jobs;


namespace ConvenienceStoreOrderService.App_Start
{
    public static class HangfireConfig
    {
        public static void Register(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("AppDbContext");

            app.UseHangfireDashboard();

            app.UseHangfireServer();

            RecurringJob.AddOrUpdate<OrderJob>(
                "auto-cancel-expired-unpaid-orders",
                job => job.AutoCancelExpiredUnpaidOrders(),
                Cron.MinuteInterval(5)
            );
            RecurringJob.AddOrUpdate<ShipmentJob>(
                "clear-expired-shipping-codes",
                job => job.ClearExpiredShippingCodes(),
                Cron.Hourly
            );
        }
    }
}
