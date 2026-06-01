using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using ConvenienceStoreOrderService.App_Start;

[assembly: OwinStartup(typeof(ConvenienceStoreOrderService.Startup))]

namespace ConvenienceStoreOrderService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HangfireConfig.Register(app);
        }
    }
}
