using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Services.Interfaces;

namespace ConvenienceStoreOrderService.Controllers
{
    public class ProductsAPIController : ApiController
    {
        private readonly IProductService _productService;

        public ProductsAPIController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        [Route("api/products")]
        public IHttpActionResult GetProductsAPI(string keyword = null,bool? isActive = null) 
        {
            var criteria = new ProductSearchCriteria
            {
                ProductKeyword = keyword,
                IsActive=isActive
            };
            var result = _productService.Search(criteria);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

    }
}