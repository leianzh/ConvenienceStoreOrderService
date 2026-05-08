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
        //丟給REPO查詢
        public List<ProductViewModel> Search(ProductSearchCriteria criteria)
        {

            if (criteria == null)
            {
                criteria = new ProductSearchCriteria();
            }
            //預設顯示上架商品
            if (!criteria.IsActive.HasValue)
            {
                criteria.IsActive = true;
            }
            return _productRepository.Search(criteria);
        }
    }
}