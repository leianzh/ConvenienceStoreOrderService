using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using ConvenienceStoreOrderService.Jobs;
using ConvenienceStoreOrderService.App_Start;
using ConvenienceStoreOrderService.Models.Helpers;


namespace ConvenienceStoreOrderService.App_Start
{
    public static class HangfireConfig
    {
        public static void Register(IAppBuilder app)
        {
            var connectionString = AppConfigHelper.GetDbConnectionString();
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(connectionString)
                .UseActivator(new UnityJobActivator(UnityConfig.Container));

            app.UseHangfireDashboard();

            app.UseHangfireServer();

            RecurringJob.AddOrUpdate<OrderJob>(
                "auto-cancel-expired-unpaid-orders",
                job => job.AutoCancelExpiredUnpaidOrders(),
                Cron.Hourly()
            );
            RecurringJob.AddOrUpdate<ShipmentJob>(
                "clear-expired-shipping-codes",
                job => job.ClearExpiredShippingCodes(),
                Cron.Hourly()
            );
            RecurringJob.AddOrUpdate<OrderJob>(
                "auto-cancel-expired-incomplete-orders",
                 job => job.AutoCancelExpiredIncompleteOrders(),
                Cron.Hourly()
            );
        }
    }
}
