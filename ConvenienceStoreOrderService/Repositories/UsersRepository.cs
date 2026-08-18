using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Constants;

namespace ConvenienceStoreOrderService.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly AppDbContext _db;
        public UsersRepository(AppDbContext db) 
        {
            _db = db;
        }
        public UserDto GetByUserId(int userId) 
        {
           var entity=_db.Users
                .FirstOrDefault( u => u.UserId == userId);
            if (entity == null)
            {
                return null;
            }
            return UserMapper.ToDto(entity);
        }
    }
}