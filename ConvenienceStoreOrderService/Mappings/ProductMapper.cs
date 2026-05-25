using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;

namespace ConvenienceStoreOrderService.Mappings
{
    public static class ProductMapper
    {
        public static ProductDto ToDto(Product entity)
        {
            return new ProductDto
            {
                ProductId = entity.ProductId,
                SellerUserId = entity.SellerUserId,
                ProductName = entity.ProductName,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl,
                Price = entity.Price,
                IsActive = entity.IsActive,
                TemperatureType = entity.TemperatureType,
                StockOnHand = entity.StockOnHand,
                StockReserved = entity.StockReserved,
                ProductVersion = entity.ProductVersion,
                
            };
        }
        public static ProductViewModel ToVM(ProductDto dto)
        {
            return new ProductViewModel
            {
                ProductId = dto.ProductId,
                SellerUserId = dto.SellerUserId,
                ProductName = dto.ProductName,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Price = dto.Price,
                IsActive = dto.IsActive,
                TemperatureType = dto.TemperatureType,
                StockOnHand = dto.StockOnHand,
                StockReserved = dto.StockReserved,
                ProductVersion = dto.ProductVersion,
            };
        }
    }
}