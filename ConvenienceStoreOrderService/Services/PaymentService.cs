using ConvenienceStoreOrderService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ConvenienceStoreOrderService.Mappings;
using ConvenienceStoreOrderService.Models.Common;
using ConvenienceStoreOrderService.Models.EFModels;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
using ConvenienceStoreOrderService.Models.DTOs;
using ConvenienceStoreOrderService.Models.Constants;
using ConvenienceStoreOrderService.Models.Helpers;
using System.Configuration;
using Newtonsoft.Json;

namespace ConvenienceStoreOrderService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly IPaymentStatusRepository _paymentStatusRepository;
        private readonly IRefundStatusRepository _refundStatusRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly AppDbContext _db;
        public PaymentService(IPaymentRepository paymentRepository, IPaymentStatusService paymentStatusService, IPaymentStatusRepository paymentStatusRepository, IRefundStatusRepository refundStatusRepository, IOrderRepository orderRepository, AppDbContext db)
        {
            _paymentRepository = paymentRepository;
            _paymentStatusService = paymentStatusService;
            _paymentStatusRepository = paymentStatusRepository;
            _refundStatusRepository = refundStatusRepository;
            _orderRepository = orderRepository;
            _db = db;
        }
        //查訂單付款方式
        public Result<string> GetPaymentMethodByOrderId(int orderId)
        {
            if (orderId <= 0)
            {
                return Result<string>.Fail(ErrorCodes.Validation, "訂單編號錯誤");
            }

            var payment = _paymentRepository.GetOrderId(orderId);

            if (payment == null)
            {
                return Result<string>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }

            if (string.IsNullOrEmpty(payment.PaymentMethod))
            {
                return Result<string>.Fail(ErrorCodes.Validation, "付款方式不可為空");
            }

            return Result<string>.Success(payment.PaymentMethod);
        }
        //根據付款狀態判斷「取消未付款」或「申請退款」
        public Result<bool> HandleCancelPayment(int orderId, string cancleReson)
        {
            var payment = _paymentRepository.GetOrderId(orderId);
            if (payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }
            //pending待付款直接取消            
            if (payment.PaymentStatusId == PaymentStatusIds.Pending)
            {
                var errorMessage = payment.CancelPending();

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return Result<bool>.Fail(ErrorCodes.Validation, errorMessage);
                }

                return Result<bool>.Success(true);
            }
            //Paid已付款，付款狀態維持 Paid，改成退款申請中
            if (payment.PaymentStatusId == PaymentStatusIds.Paid)
            {
                var errorMessage = payment.RequestRefund(
            RefundStatusIds.Requested,
            payment.Amount,
            cancleReson
        );
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return Result<bool>.Fail(ErrorCodes.Validation, errorMessage);
                }

                return Result<bool>.Success(true);
            }
            return Result<bool>.Fail(ErrorCodes.Validation, "此付款狀態不能取消或退款");

        }
        //檢查能不能出貨
        public Result<bool> CheckCanShip(int orderId)
        {
            var payment = _paymentRepository.GetOrderId(orderId);
            if (payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }
            //COD
            if (payment.PaymentMethod == PaymentMethodName.COD)
            {
                return Result<bool>.Success(true);
            }
            //線上付款必須 Paid 才能出貨
            if (payment.PaymentStatusId == 2)
            {
                return Result<bool>.Success(true);
            }
            else
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "線上付款尚未完成，不能出貨");
            }

        }
        //線上付款成功
        public Result<bool> MarkPaid(int orderId)
        {
            var payment = _paymentRepository.GetOrderId(orderId);
            if (payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");

            }

            var result = payment.MarkPaid(payment.PaymentStatusId, payment.TradeNo, payment.RawCallBack);
            if (!string.IsNullOrEmpty(result))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, result);
            }
            _paymentRepository.SaveChanges();
            return Result<bool>.Success(true, "付款成功");
        }
        //COD取貨付款成功
        public Result<bool> MarkCodPaidWhenPickedUp(int orderId)
        {
            var payment = _paymentRepository.GetOrderId(orderId);

            if (payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }

            // 如果不是 COD，就不用處理
            if (payment.PaymentMethod != PaymentMethodName.COD)
            {
                return Result<bool>.Success(true);
            }

            var errorMessage = payment.MarkPaidForCodPickedUp();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
            }

            return Result<bool>.Success(true, "COD 取貨付款成功");
        }
        //申請退款
        public Result<bool> RequestRefund(int orderId, string reason)
        {
            var payment = _paymentRepository.GetOrderId(orderId);

            if (payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }

            var paymentStatus = _paymentStatusService.GetById(payment.PaymentStatusId);

            if (!paymentStatus.IsSuccess)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "查詢付款狀態失敗");
            }

            if (paymentStatus.Data.PaymentStatusCode != "Paid")
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "只有已付款訂單才能申請退款");
            }

            var refundStatus = _refundStatusRepository.GetByCode("Requested");

            if (refundStatus == null)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "找不到退款狀態：Requested");
            }

            payment.RequestRefund(
                refundStatus.RefundStatusId,
                payment.Amount,
                reason
            );

            _paymentRepository.SaveChanges();

            return Result<bool>.Success(true);
        }
        //退款完成
        public Result<bool> MarkRefunded(int orderId, string refundProviderTradeNo, string rawResponse)
        {
            var payment = _paymentRepository.GetOrderId(orderId);
            if (payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }
            // 找有沒有Requested
            var requestedStatus = _refundStatusRepository.GetByCode
                ("Requested");
            if (requestedStatus == null)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "找不到退款狀態：Requested");
            }
            //找有沒有Refunded 
            var refundedStatus = _refundStatusRepository.GetByCode("Refunded");
            if (refundedStatus == null)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "找不到退款狀態：Refunded");
            }
            var errorMessage = payment.MarkRefunded(
                requestedStatus.RefundStatusId,
                refundedStatus.RefundStatusId,
                refundProviderTradeNo,
                rawResponse
                );
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
            }

            _paymentRepository.SaveChanges();

            return Result<bool>.Success(true, "退款已完成");
        }


        //建立藍新付款請求
        public Result<NewebPayMpgRequestDto> CreateCreditCardOnceMpgRequest(int orderId)
        {
            var tran = _db.Database.BeginTransaction();
            try
            {
                //  找訂單
                var order = _orderRepository.GetEntityById(orderId);

                if (order == null)
                {
                    return Result<NewebPayMpgRequestDto>.Fail(
                        ErrorCodes.NotFound,
                        "找不到訂單"
                    );

                }

                // 找付款資料
                var payment = _paymentRepository.GetOrderId(orderId);

                if (payment == null)
                {
                    return Result<NewebPayMpgRequestDto>.Fail(
                        ErrorCodes.NotFound,
                        "找不到付款資料"
                    );
                }

                // 只有 Pending 可以前往付款
                if (payment.PaymentStatusId != PaymentStatusIds.Pending)
                {
                    return Result<NewebPayMpgRequestDto>.Fail(
                        ErrorCodes.Validation,
                        "只有待付款訂單可以進行信用卡付款"
                    );
                }

                // 只允許信用卡付款方式
                if (payment.PaymentMethod != PaymentMethodName.CreditCard)
                {
                    return Result<NewebPayMpgRequestDto>.Fail(
                        ErrorCodes.Validation,
                        "此訂單不是信用卡付款"
                    );
                }

                // 讀 Web.config 設定
                var merchantId = ConfigurationManager.AppSettings["NewebPay.MerchantID"];
                var hashKey = ConfigurationManager.AppSettings["NewebPay.HashKey"];
                var hashIV = ConfigurationManager.AppSettings["NewebPay.HashIV"];
                var version = ConfigurationManager.AppSettings["NewebPay.Version"];
                var mpgUrl = ConfigurationManager.AppSettings["NewebPay.MpgUrl"];
                var returnUrl = ConfigurationManager.AppSettings["NewebPay.ReturnUrl"];
                var notifyUrl = ConfigurationManager.AppSettings["NewebPay.NotifyUrl"];

                if (string.IsNullOrWhiteSpace(merchantId) ||
                    string.IsNullOrWhiteSpace(hashKey) ||
                    string.IsNullOrWhiteSpace(hashIV))
                {
                    return Result<NewebPayMpgRequestDto>.Fail(
                        ErrorCodes.SystemError,
                        "藍新金流設定不完整"
                    );
                }

                //藍新需要的參數
                var timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                var merchantOrderNo = order.OrderNo;
                var amount = payment.Amount;
                var tradeInfoParams = new Dictionary<string, string>
                {
                    { "MerchantID", merchantId },
                    { "RespondType", "JSON" },
                    { "TimeStamp", timeStamp },
                    { "Version", version },
                    { "MerchantOrderNo", merchantOrderNo },
                    { "Amt", amount.ToString() },
                    { "ItemDesc", "ConvenienceStoreOrder" },
                    { "ReturnURL", returnUrl },
                    { "NotifyURL", notifyUrl },

                    // 只開信用卡一次付清
                    { "CREDIT", "1" },
                 };

                // 組成 query string
                var tradeInfoPlainText = BuildQueryString(tradeInfoParams);
                if (string.IsNullOrWhiteSpace(tradeInfoPlainText))
                {
                    tran.Rollback();
                }

                // TradeInfo
                var tradeInfo = NewebPayCryptoHelper.EncryptTradeInfo(
                    tradeInfoPlainText,
                    hashKey,
                    hashIV
                );
                if (tradeInfo == null)
                {
                    tran.Rollback();
                }
                // TradeSha
                var tradeSha = NewebPayCryptoHelper.GenerateTradeSha(
                    tradeInfo,
                    hashKey,
                    hashIV
                );
               

                if (tradeSha == null)
                {
                    tran.Rollback();
                }

                //給 View 自動 POST 到藍新
                var dto = new NewebPayMpgRequestDto
                {
                    MpgUrl = mpgUrl,
                    MerchantID = merchantId,
                    TradeInfo = tradeInfo,
                    TradeSha = tradeSha,
                    Version = version,
                    EncryptType = "0"
                };
                if (dto == null)
                {
                    tran.Rollback();
                }

                tran.Commit();
                return Result<NewebPayMpgRequestDto>.Success(dto);



            }
            catch (Exception ex)
            {
                tran.Rollback();
                return Result<NewebPayMpgRequestDto>.Fail(
                    ErrorCodes.SystemError,
                    "藍新付款請求失敗：" + ex.Message
                );
            }
            finally
            {
                tran.Dispose();
            }
        }
        //組成 query string
        string BuildQueryString(Dictionary<string, string> parameters)
        {
            var items = new List<string>();

            foreach (var item in parameters)
            {
                var key = HttpUtility.UrlEncode(item.Key);
                var value = HttpUtility.UrlEncode(item.Value);

                items.Add($"{key}={value}");
            }

            return string.Join("&", items);
        }
        //處理 Notify，改付款狀態
        public Result<bool> HandleNewebPayNotify(string tradeInfo, string tradeSha)
        {
            var tran = _db.Database.BeginTransaction();
            try
            {
                // 藍新TradeInfo、TradeSha
                if (string.IsNullOrWhiteSpace(tradeInfo))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳 TradeInfo 為空"
                    );
                }

                if (string.IsNullOrWhiteSpace(tradeSha))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳 TradeSha 為空"
                    );
                }

                // 讀取 HashKey、HashIV
                var hashKey = ConfigurationManager.AppSettings["NewebPay.HashKey"];
                var hashIV = ConfigurationManager.AppSettings["NewebPay.HashIV"];

                if (string.IsNullOrWhiteSpace(hashKey) ||
                    string.IsNullOrWhiteSpace(hashIV))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.SystemError,
                        "藍新 HashKey / HashIV 設定不完整"
                    );
                }

                // 驗證 TradeSha                
                var checkTradeSha = NewebPayCryptoHelper.GenerateTradeSha(
                    tradeInfo,
                    hashKey,
                    hashIV
                );
             

                if (!string.Equals(
                        checkTradeSha,
                        tradeSha,
                        StringComparison.OrdinalIgnoreCase))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "TradeSha 驗證失敗"
                    );
                }

                // 解密 TradeInfo
                var json = NewebPayCryptoHelper.DecryptTradeInfo(
                    tradeInfo,
                    hashKey,
                    hashIV
                );

                if (string.IsNullOrWhiteSpace(json))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "TradeInfo 內容為空"
                    );
                }

                // 把 JSON 轉成 DTO
                var notify = JsonConvert.DeserializeObject<NewebPayNotifyDto>(json);

                Console.WriteLine( notify);
                if (notify == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳資料格式錯誤"
                    );
                }

                if (notify.Result == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳 Result 為空"
                    );
                }

                // 判斷付款是否成功，Status = SUCCESS
                if (notify.Status != "SUCCESS")
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "藍新付款未成功：" + notify.Message
                    );
                }

                //  PaymentType =CREDIT
                if (notify.Result.PaymentType != "CREDIT")
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "此通知不是信用卡付款：" + notify.Result.PaymentType
                    );
                }

                // 用 MerchantOrderNo 找付款資料
                var payment = _paymentRepository.GetByOrderNo(
                    notify.Result.MerchantOrderNo
                );

                if (payment == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.NotFound,
                        "找不到付款資料，MerchantOrderNo：" + notify.Result.MerchantOrderNo
                    );
                }

                // 檢查金額 Amt 是否一致
                if (payment.Amount != notify.Result.Amt)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "付款金額不一致"
                    );
                }
              

                // 如果不是 Pending，也不允許改 Paid
                if (payment.PaymentStatusId != PaymentStatusIds.Pending)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "此付款狀態不可改為已付款"
                    );
                }

                // 改成已付款
                var errorMessage = payment.MarkPaid(
                    PaymentStatusIds.Paid,
                    notify.Result.TradeNo,
                    json
                );

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        errorMessage
                    );
                }

                // 存 DB
                _paymentRepository.SaveChanges();
                tran.Commit();
                return Result<bool>.Success(
                    true,
                    "藍新付款通知處理成功"
                );

            }
            catch (Exception ex)
            {
                //return Result<bool>.Fail(
                //    ErrorCodes.SystemError,
                //    "處理藍新付款通知失敗：" + ex.Message
                //);
                tran.Rollback();
                return Result<bool>.Fail(
                    ErrorCodes.SystemError,
                            "處理藍新付款通知失敗：" + ex.ToString()
                                        );
            }
            finally { tran.Dispose(); }

        }
    }
}