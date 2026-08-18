using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Mappings
{
    public class UserMapper
    {
        public static UserDto ToDto(Users entity)
        {
            return new UserDto
            {
                UserId = entity.UserId,
                UserEmail = entity.UserEmail,
                UserName = entity.UserName,
                UserPhone = entity.UserPhone,
                CreatedAt = entity.CreatedAt,
            };
        }
        public static UserViewModel ToVM(UserDto dto)
        {
            return new UserViewModel
            {
                UserId = dto.UserId,
                UserEmail = dto.UserEmail,
                UserName = dto.UserName,
                UserPhone = dto.UserPhone,
                CreatedAt = dto.CreatedAt,
            };
        }
    }
}