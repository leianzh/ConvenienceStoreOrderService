using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Services
{
    public class ProductService : IProductService
    {
        private IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public List<ProductViewModel> GetProducts()
        {
            return _productRepository.GetProducts();
        }
    }
}