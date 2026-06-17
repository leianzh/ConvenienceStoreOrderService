using ConvenienceStoreOrderService.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.EFModels;

namespace ConvenienceStoreOrderService.Services.Interfaces
{
    public interface IPaymentService
    {
        Result<bool> HandleCancelPayment(int orderId, string cancelReason);
        Result<bool> CheckCanShip(int orderId);
        Result<bool> MarkPaid(int orderId);
        Result<bool> MarkCodPaidWhenPickedUp(int orderId);
        Result<bool> RequestRefund(int orderId, string reason);
        Result<bool> MarkRefunded(int orderId, string refundProviderTradeNo, string rawResponse);
        Result<NewebPayMpgRequestDto> CreateCreditCardOnceMpgRequest(int orderId);
        Result<bool> HandleNewebPayNotify(string tradeInfo, string tradeSha);


    }
}
