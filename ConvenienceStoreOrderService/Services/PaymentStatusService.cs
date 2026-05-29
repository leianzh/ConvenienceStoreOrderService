using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.DTOs;

namespace ConvenienceStoreOrderService.Services
{
    public class PaymentStatusService : IPaymentStatusService
    {
         private readonly IPaymentStatusRepository _paymentStatusRepository;

    public PaymentStatusService(IPaymentStatusRepository paymentStatusRepository)
    {
        _paymentStatusRepository = paymentStatusRepository;
    }

        public Result<PaymentStatusDto> GetByCode(string paymentStatusCode)
        {
            var dto = _paymentStatusRepository.GetByCode(paymentStatusCode);
            if (dto == null)
            {
                return Result<PaymentStatusDto>.Fail(ErrorCodes.Validation, "找不到付款狀態");
            }
            return Result<PaymentStatusDto>.Success(dto);
        }

        public Result<PaymentStatusDto> GetById(int paymentStatusId)
        {
            var dto = _paymentStatusRepository.GetById(paymentStatusId);
            if(dto == null) 
            {
                return Result<PaymentStatusDto>.Fail(ErrorCodes.Validation, "找不到付款狀態");
            }
            return Result<PaymentStatusDto>.Success(dto);
        }

        public Result< List<PaymentStatusViewModel>> GetPaymentStatusOptions()
    {
            try 
            {
                var dtoList = _paymentStatusRepository.GetPaymentStatusOptions();
                if (dtoList == null) 
                {
                    return Result<List<PaymentStatusViewModel>>.Fail(
                        ErrorCodes.Validation, "查詢條件不可為空");
                }
                if (dtoList == null || !dtoList.Any())
                {
                    return Result<List<PaymentStatusViewModel>>.Fail(
                        ErrorCodes.NotFound, "查無付款狀態選項");
                }
                return Result<List<PaymentStatusViewModel>>.Success(
                    dtoList.Select(x => PaymentStatusMapper.ToVm(x))
        .ToList(), "查詢成功");
            }
            catch (Exception)
            {
                return Result<List<PaymentStatusViewModel>>.Fail(
                    ErrorCodes.SystemError, "系統發生錯誤，請稍後再試");
            }
           

            
        }

}
}
