using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.Constants;
using System.Data.Entity.Infrastructure;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Models.ViewModels;
using System.Web.Http.Results;

namespace ConvenienceStoreOrderService.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly AppDbContext _db;
        public UsersService (IUsersRepository usersRepository, AppDbContext db)
        {
            _usersRepository = usersRepository;
            _db = db;
        }
        public Result< UserViewModel> GetUsers(int userId)
        {
            var dto = _usersRepository.GetByUserId(userId);
            if (dto == null)
            {
                return Result<UserViewModel>.Fail(
                    ErrorCodes.NotFound,
                    "找不到使用者資料"
                );
            }
            var vm = UserMapper.ToVM(dto);
                
            return Result<UserViewModel>.Success(vm);

        }
    }
}