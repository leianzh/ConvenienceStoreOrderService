using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.DTOs;

namespace ConvenienceStoreOrderService.Services.Interfaces
{
    public interface IUsersService
    {
        Result<UserViewModel> GetUsers(int userId);
    }
}
