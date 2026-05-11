using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.Common;

namespace ConvenienceStoreOrderService.Services.Interfaces
{
    public interface IProductService
    {
        List<ProductViewModel> GetProducts();
        Result< List<ProductViewModel> >Search(ProductSearchCriteria criteria);
    }
}
