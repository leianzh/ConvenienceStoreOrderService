using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;

namespace ConvenienceStoreOrderService.Mappings
{
    public static class ProductMapper
    {
        public static ProductDto ToDto(Product entity)
        {
            return new ProductDto
            {
                ProductId = entity.ProductId,
                ProductName = entity.ProductName,
                Price = entity.Price,
                IsActive = entity.IsActive,
            };
        }
    }
}