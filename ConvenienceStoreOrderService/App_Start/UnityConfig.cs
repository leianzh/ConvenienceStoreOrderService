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
            container.RegisterType<IProductService, ProductService>();//DIµù¥U
            container.RegisterType<IProductRepository, ProductRepository>();//DIµù¥U
            container.RegisterType<AppDbContext>();//DIµù¥U

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}