using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.DTOs;

namespace ConvenienceStoreOrderService.Repositories.Interfaces
{
    public interface IProductRepository
    {
        List<ProductViewModel> GetProducts();
        List<ProductViewModel> Search(ProductSearchCriteria criteria);
        List<ProductDto> GetProductsAPI();
        List<ProductDto> SearchApi(ProductSearchCriteria criteria);
    }
}