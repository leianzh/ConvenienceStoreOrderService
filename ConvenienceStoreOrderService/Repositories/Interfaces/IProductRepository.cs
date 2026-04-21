using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Repositories.Interfaces
{
    public interface IProductRepository
    {
        List<ProductViewModel> GetProducts();
    }
}