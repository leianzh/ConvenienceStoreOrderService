using System.Web.Mvc;
using Unity;
using Unity.Mvc5;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Services;
using ConvenienceStoreOrderService.Services.Interfaces;
using ConvenienceStoreOrderService.Models.EFModels;

namespace ConvenienceStoreOrderService
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<IProductService, ProductService>();//DI딩쩤
            container.RegisterType<IProductRepository, ProductRepository>();//DI딩쩤
            container.RegisterType<AppDbContext>();//DI딩쩤
            container.RegisterType<IPaymentStatusService, PaymentStatusService>();//DI딩쩤
            container.RegisterType<IPaymentStatusRepository, PaymentStatusRepository>();//DI딩쩤
            

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}