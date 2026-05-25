using System.Web.Mvc;
using Unity;
using Unity.Mvc5;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Services;
using ConvenienceStoreOrderService.Services.Interfaces;
using ConvenienceStoreOrderService.Models.EFModels;
using System.ComponentModel;

namespace ConvenienceStoreOrderService
{
    public static class UnityConfig
    {
        private static IUnityContainer _container;
        public static IUnityContainer Container
        {
            get
            {
                if (_container == null)
                {
                    _container = new UnityContainer();
                    RegisterTypes(_container);
                }

                return _container;
            }
        }
        public static void RegisterComponents()
        {
           
            DependencyResolver.SetResolver(new UnityDependencyResolver(Container));
        }
        public static void RegisterTypes(IUnityContainer container)
        {


            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<IProductService, ProductService>();//DI딩쩤
            container.RegisterType<IProductRepository, ProductRepository>();//DI딩쩤
            container.RegisterType<AppDbContext>();//DI딩쩤
            container.RegisterType<IPaymentStatusService, PaymentStatusService>();//DI딩쩤
            container.RegisterType<IPaymentStatusRepository, PaymentStatusRepository>();//DI딩쩤
            container.RegisterType<IOrderService, OrderService>();

            container.RegisterType<IOrderRepository, OrderRepository>();

            container.RegisterType<IOrderStatusService,OrderStatusService>();
            container.RegisterType<IOrderStatusRepository, OrderStatusRepository>();
            container.RegisterType<IShipmentRepository,ShipmentRepository >();
            container.RegisterType<IShipmentService, ShipmentService>();
            container.RegisterType<IOrderDetailService,OrderDetailService>();
            container.RegisterType<IOrderDetailRepository, OrderDetailRepository>();




            //DependencyResolver.SetResolver(new UnityDependencyResolver(Container));
        }
    }
}