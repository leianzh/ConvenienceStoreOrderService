using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Mappings;


namespace ConvenienceStoreOrderService.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;

        public ProductRepository(AppDbContext db)
        {
            _db = db;
        }
        public List<ProductViewModel> GetProducts()
        {
                return _db.Products
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price
                })
                .ToList();
            
        }
        public List<ProductViewModel> Search(ProductSearchCriteria criteria ) 
        {
            var query=_db.Products.AsQueryable();
            //查關鍵字
            if (!string.IsNullOrWhiteSpace(criteria.ProductKeyword))
            {
                query = query.Where(p => p.ProductName.Contains(criteria.ProductKeyword));
            }
            //查上下架
            if (criteria.IsActive.HasValue) 
            {
                query = query.Where(p => p.IsActive == criteria.IsActive.Value);
            }
            //查溫層
            if (criteria.TemperatureType.HasValue)
            {
                query = query.Where(p => p.TemperatureType == criteria.TemperatureType.Value);
            }
            //查價格
            if (criteria.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= criteria.MinPrice.Value);
            }

            if (criteria.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= criteria.MaxPrice.Value);
            }
            return query
                .Select(p=> new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price

                })
                .ToList();
        }

        public List<ProductDto> GetProductsAPI ()
        {
            return _db.Products
                .AsEnumerable()
                .Select(p => ProductMapper.ToDto(p))
                .ToList();


        }

        public List<ProductDto> SearchApi(ProductSearchCriteria criteria)
        {
            var query = _db.Products.AsQueryable();
            //查關鍵字
            if(! string.IsNullOrEmpty(criteria.ProductKeyword) ) 
            {
                query =query.Where(p =>p.ProductName.Contains(criteria.ProductKeyword));
            }
            //查上下架
            if (criteria.IsActive.HasValue)
            {
                query = query.Where(p =>p.IsActive == criteria.IsActive);
            }
            return query
                .AsEnumerable()
                .Select(p =>ProductMapper.ToDto(p))
                .ToList();
        }
    }
}