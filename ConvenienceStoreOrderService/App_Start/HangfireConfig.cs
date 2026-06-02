using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using ConvenienceStoreOrderService.Jobs;
using ConvenienceStoreOrderService.App_Start;


namespace ConvenienceStoreOrderService.App_Start
{
    public static class HangfireConfig
    {
        public static void Register(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("AppDbContext")
                .UseActivator(new UnityJobActivator(UnityConfig.Container));

            app.UseHangfireDashboard();

            app.UseHangfireServer();

            RecurringJob.AddOrUpdate<OrderJob>(
                "auto-cancel-expired-unpaid-orders",
                job => job.AutoCancelExpiredUnpaidOrders(),
                Cron.MinuteInterval(1)
            );
            RecurringJob.AddOrUpdate<ShipmentJob>(
                "clear-expired-shipping-codes",
                job => job.ClearExpiredShippingCodes(),
                Cron.Hourly
            );
        }
    }
}
