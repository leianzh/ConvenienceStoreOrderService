using ConvenienceStoreOrderService.Models.EFModels;
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
        public ActionResult List()
        {
            var products = _productService.GetProducts();
            return View(products);
        }
       

    }
}