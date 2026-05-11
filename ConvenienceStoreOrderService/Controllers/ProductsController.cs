using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Services;
using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConvenienceStoreOrderService.Controllers
{
    public class ProductsController : Controller
    {
        private IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
        //GET: Products
        public ActionResult List(ProductSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new ProductSearchCriteria();
            }
           
            criteria.IsActiveOptions = new List<SelectListItem>
            {
                new SelectListItem {Text = "全部", Value = ""},
                new SelectListItem {Text = "上架", Value = "true"},
                new SelectListItem { Text = "下架", Value = "false"}

            };
            criteria.TemperatureTypeOptions = new List<SelectListItem>
            {
                new SelectListItem { Text = "全部", Value = "" },
                new SelectListItem { Text = "常溫", Value = "1" },
                new SelectListItem { Text = "冷藏", Value = "2" },
                new SelectListItem { Text = "冷凍", Value = "3" }
            };
            var result = _productService.Search(criteria);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                criteria.Products = new List<ProductViewModel>();
                return View(criteria);
            }

            criteria.Products = result.Data;

            return View(criteria);

        }


    }
}