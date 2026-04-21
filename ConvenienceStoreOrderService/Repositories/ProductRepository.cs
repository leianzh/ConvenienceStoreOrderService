using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;

namespace ConvenienceStoreOrderService.Repositories
{
    public class ProductRepository : IProductRepository
    {
        public List<ProductViewModel> GetProducts()
        {
            return new List<ProductViewModel>
            {
                new ProductViewModel { ProductId = 1, ProductName = "紅茶", Price = 30 },
                new ProductViewModel { ProductId = 2, ProductName = "奶茶", Price = 45 },
                new ProductViewModel { ProductId = 3, ProductName = "綠茶", Price = 25 }
            };
        }
    }
}