using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;


namespace ConvenienceStoreOrderService.Repositories
{
    public class ProductRepository : IProductRepository
    {
        public List<ProductViewModel> GetProducts()
        {
            using(var db= new AppDbContext())
            {
                return db.Products
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price
                })
                .ToList();
            }
        }
    }
}