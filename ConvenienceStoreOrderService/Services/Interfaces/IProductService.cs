using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Services.Interfaces
{
    public interface IProductService
    {
        List<ProductViewModel> GetProducts();
    }
}
