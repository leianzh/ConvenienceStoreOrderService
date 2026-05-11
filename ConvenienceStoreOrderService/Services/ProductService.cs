using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.Common;

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
        public Result< List<ProductViewModel>> Search(ProductSearchCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    return Result<List<ProductViewModel>>.Fail(
                ErrorCodes.Validation,
                "查詢條件不可為空");
                }
                if (criteria.MinPrice.HasValue && criteria.MaxPrice.HasValue &&
             criteria.MinPrice.Value > criteria.MaxPrice.Value)
                {
                    return Result<List<ProductViewModel>>.Fail(
                        ErrorCodes.Validation,
                        "最低價格不能大於最高價格"
                    );
                }
                var products = _productRepository.Search(criteria);

                if (products == null || !products.Any())
                {
                    return Result<List<ProductViewModel>>.Fail(
                        ErrorCodes.NotFound,
                        "查無符合條件的商品"
                    );
                }

                return Result<List<ProductViewModel>>.Success(
                    products, "查詢成功");

            }
            catch (Exception ) 
            {
                return Result<List<ProductViewModel>>.Fail
                    (ErrorCodes.SystemError, "系統發生錯誤，請稍後再試");
            }

            
        }
    }
}