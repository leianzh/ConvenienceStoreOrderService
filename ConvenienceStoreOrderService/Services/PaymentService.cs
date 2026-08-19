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
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Microsoft.Ajax.Utilities;

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
        //根據付款狀態判斷「取消未付款 / 取消授權 / 申請退款」
        public Result<bool> ReturnOrCancelPayment(int orderId, string cancleReson)
        {
            var payment = _paymentRepository.GetOrderId(orderId);
            if (payment == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
            }


            //pending，COD 未取貨退回         
            if (payment.PaymentStatusId == PaymentStatusIds.Pending)
            {
                var errorMessage = payment.CancelUnpaidReturn(cancleReson);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return Result<bool>.Fail(ErrorCodes.Validation, errorMessage);
                }

                return Result<bool>.Success(true, "COD 未取貨退回，未收款，不需退款");
            }
            // Paid：信用卡授權成功，但尚未請款
            if (payment.PaymentStatusId == PaymentStatusIds.Paid &&
                payment.PaymentMethod == PaymentMethodName.CreditCard &&
                 payment.IsCaptured == false)
            {
                var cancelAuthResult = CancelCreditCardAuthorization(orderId);

                if (!cancelAuthResult.IsSuccess)
                {
                    return Result<bool>.Fail(
                        cancelAuthResult.ErrorCode,
                        cancelAuthResult.Message
                    );
                }

                return Result<bool>.Success(true, "信用卡取消授權成功");
            }
            //Paid已付款，付款狀態維持 Paid，改成退款申請中，信用卡需已請款
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

                return Result<bool>.Success(true,"已付款訂單已建立退款申請");
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
            //線上付款必須 Paid +授權成功才能出貨
            if (payment.PaymentMethod == PaymentMethodName.CreditCard)
            {
                if (payment.PaymentStatusId != PaymentStatusIds.Paid)
                {
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "信用卡尚未付款成功，不能出貨"
                    );
                }
                if (!payment.IsCaptured)
                {
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "信用卡尚未請款成功，不能出貨"
                    );
                }
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
        //信用卡請款(出貨時呼叫)
        public Result<bool> CaptureCreditCardPayment(int orderId)
        {
            try
            {
                var payment = _paymentRepository.GetOrderId(orderId);
                if (payment == null)
                {
                    return Result<bool>.Fail(ErrorCodes.NotFound, "找不到付款資料");
                }
                // COD 不需要請款
                if (payment.PaymentMethod == PaymentMethodName.COD)
                {
                    return Result<bool>.Success(true, "COD 不需要信用卡請款");
                }
                if (payment.PaymentStatusId != PaymentStatusIds.Paid)
                {
                    return Result<bool>.Fail(ErrorCodes.Validation, "信用卡尚未授權成功，不能出貨");
                }
                // 已請款就不要重複呼叫藍新
                if (payment.IsCaptured)
                {
                    return Result<bool>.Success(true, "信用卡已請款，不需重複請款");
                }
                //請款closeType = 1
                var closeResult = CloseTrade(orderId, 1);
                if (!closeResult.IsSuccess)
                {
                    payment.CaptureRequestedAt = DateTime.Now;
                    payment.CaptureStatusCode = "Failed";
                    payment.CaptureMessage = closeResult.Message;
                    payment.CaptureRawResponse = null;
                    payment.UpdatedAt = DateTime.Now;

                    _paymentRepository.SaveChanges();

                    return Result<bool>.Fail(
                        closeResult.ErrorCode,
                        "信用卡請款失敗：" + closeResult.Message
                    );
                }
                var errorMessage = payment.MarkCaptured(
                                                    closeResult.Data.RawJson,
                                                    closeResult.Data.Message);
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return Result<bool>.Fail(ErrorCodes.Validation, errorMessage);
                }
                _paymentRepository.SaveChanges();
                return Result<bool>.Success(true, "信用卡請款成功");
            }
            catch(Exception ex) 
            {
                return Result<bool>.Fail(ErrorCodes.Validation, ex.Message);
            }
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
        //根據付款方式決定怎麼退款
        public Result<bool> ProcessRefund(int orderId)
        {
            var payment =_paymentRepository.GetOrderId(orderId);
            if(payment == null)
            {
                return Result<bool>.Fail(
                    ErrorCodes.NotFound, "找不到付款資料");
            }
            //必須已經是Requested才可以走退款
            if(payment.RefundStatusId != RefundStatusIds.Requested)
            {
                return Result<bool>.Fail(
                    ErrorCodes.Validation,
                    "退款狀態必須是 Requested");
            }
            // COD不呼叫藍新
            if (payment.PaymentMethod == PaymentMethodName.COD)
            {
                return Result<bool>.Success(
                    true,
                    "COD 等待人工退款");
            }
            // 信用卡：呼叫藍新退款api
            if (payment.PaymentMethod == PaymentMethodName.CreditCard)
            {
                if (!payment.IsCaptured)
                {
                    return Result<bool>.Fail(
                        ErrorCodes.Validation,
                        "信用卡尚未請款，不能退款，請改走取消授權"
                    );
                }
                var closeResult = CloseTrade(orderId, 2);

                if (!closeResult.IsSuccess)
                {
                    return Result<bool>.Fail(
                        closeResult.ErrorCode,
                        closeResult.Message);
                }
                //退款申請成功後，立即查一次藍新交易狀態
                var queryResult =QueryTradeInfo(orderId);
                if (!queryResult.IsSuccess)
                {
                    
                    return Result<bool>.Success(
                        true,
                        "藍新退款申請已送出，目前尚未確認退款結果"
                    );
                }
                //藍新確認退款完成
                if (queryResult.Data.BackStatus == "3")
                {
                    var refundedResult = MarkRefunded(
                        orderId,
                        queryResult.Data.TradeNo,
                        queryResult.Data.RawJson
                    );

                    if (!refundedResult.IsSuccess)
                    {
                        return refundedResult;
                    }

                    return Result<bool>.Success(
                        true,
                        "藍新退款已完成"
                    );
                }
                return Result<bool>.Success(
                    true,
                    "藍新退款申請已送出，退款尚在處理中");
            }

            return Result<bool>.Fail(
                ErrorCodes.Validation,
                "不支援的付款方式");
        }
        //COD 人工退款完成
        public Result<bool> CompleteCODRefund(int orderId)
        {
            var payment = _paymentRepository.GetOrderId(orderId);

            if (payment == null)
            {
                return Result<bool>.Fail(
                    ErrorCodes.NotFound,
                    "找不到付款資料");
            }

            // 只允許 COD 人工退款
            if (payment.PaymentMethod != PaymentMethodName.COD)
            {
                return Result<bool>.Fail(
                    ErrorCodes.Validation,
                    "信用卡退款不可人工標記完成，必須以藍新退款結果為準");
            }

            // 必須已經申請退款
            if (payment.RefundStatusId != RefundStatusIds.Requested)
            {
                return Result<bool>.Fail(
                    ErrorCodes.Validation,
                    "只有退款申請中的 COD 訂單才能完成退款");
            }

            return MarkRefunded(
                orderId,
                "MANUAL_REFUND_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                "COD 人工退款完成"
            );
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
                var merchantId = AppConfigHelper.GetRequiredSetting(
                                "COS_NEWEBPAY_MERCHANT_ID");

                var hashKey = AppConfigHelper.GetRequiredSetting(
                              "COS_NEWEBPAY_HASH_KEY");

                var hashIV = AppConfigHelper.GetRequiredSetting(
                            "COS_NEWEBPAY_HASH_IV");
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
                var hashKey = AppConfigHelper.GetRequiredSetting(
                              "COS_NEWEBPAY_HASH_KEY");
                var hashIV = AppConfigHelper.GetRequiredSetting(
                            "COS_NEWEBPAY_HASH_IV");

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
                
                tran.Rollback();
                return Result<bool>.Fail(
                    ErrorCodes.SystemError,
                            "處理藍新付款通知失敗：" + ex.ToString()
                                        );
            }
            finally { tran.Dispose(); }

        }
        //藍新單筆交易查詢 API
        public Result<NewebPayQueryResultViewModel> QueryTradeInfo(int orderId)
        {
            try
            {
                var order = _orderRepository.GetEntityById(orderId);

                if (order == null)
                {
                    return Result<NewebPayQueryResultViewModel>.Fail(
                        ErrorCodes.NotFound,
                        "找不到訂單"
                    );
                }

                var payment = _paymentRepository.GetOrderId(orderId);

                if (payment == null)
                {
                    return Result<NewebPayQueryResultViewModel>.Fail(
                        ErrorCodes.NotFound,
                        "找不到付款資料"
                    );
                }

                if (payment.PaymentMethod != PaymentMethodName.CreditCard)
                {
                    return Result<NewebPayQueryResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "此訂單不是信用卡付款"
                    );
                }

                
                var merchantId = AppConfigHelper.GetRequiredSetting(
                               "COS_NEWEBPAY_MERCHANT_ID");
                var hashKey = AppConfigHelper.GetRequiredSetting(
                              "COS_NEWEBPAY_HASH_KEY");
                var hashIV = AppConfigHelper.GetRequiredSetting(
                            "COS_NEWEBPAY_HASH_IV");
                var queryUrl = ConfigurationManager.AppSettings["NewebPay.QueryTradeInfoUrl"];

                if (string.IsNullOrWhiteSpace(queryUrl))
                {
                    queryUrl = "https://ccore.newebpay.com/API/QueryTradeInfo";
                }

                if (string.IsNullOrWhiteSpace(merchantId) ||
                    string.IsNullOrWhiteSpace(hashKey) ||
                    string.IsNullOrWhiteSpace(hashIV))
                {
                    return Result<NewebPayQueryResultViewModel>.Fail(
                        ErrorCodes.SystemError,
                        "藍新金流設定不完整"
                    );
                }

                var merchantOrderNo = order.OrderNo;

                // 藍新金額要純整數
                var amount = Convert.ToInt32(payment.Amount);
                var amountText = amount.ToString();

                var checkValue = NewebPayCryptoHelper.GenerateQueryCheckValue(
                    amountText,
                    merchantId,
                    merchantOrderNo,
                    hashKey,
                    hashIV
                );

                if (string.IsNullOrWhiteSpace(checkValue))
                {
                    return Result<NewebPayQueryResultViewModel>.Fail(
                        ErrorCodes.SystemError,
                        "產生 CheckValue 失敗"
                    );
                }

                var requestDto = new NewebPayQueryRequestDto
                {
                    QueryUrl = queryUrl,
                    MerchantID = merchantId,
                    Version = "1.3",
                    RespondType = "JSON",
                    CheckValue = checkValue,
                    TimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                    MerchantOrderNo = merchantOrderNo,
                    Amt = amountText
                };

                
                var formData = new Dictionary<string, string>
            {
                { "MerchantID", requestDto.MerchantID },
                { "Version", requestDto.Version },
                { "RespondType", requestDto.RespondType },
                { "CheckValue", requestDto.CheckValue },
                { "TimeStamp", requestDto.TimeStamp },
                { "MerchantOrderNo", requestDto.MerchantOrderNo },
                { "Amt", requestDto.Amt }
            };

                string responseJson;

                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(formData);

                    var httpResponse = client
                        .PostAsync(requestDto.QueryUrl, content)
                        .GetAwaiter()
                        .GetResult();

                    responseJson = httpResponse.Content
                        .ReadAsStringAsync()
                        .GetAwaiter()
                        .GetResult();

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        return Result<NewebPayQueryResultViewModel>.Fail(
                            ErrorCodes.SystemError,
                            "藍新單筆交易查詢 HTTP 失敗：" + responseJson
                        );
                    }
                }

                // 丟給接收 Service 處理 JSON
                return HandleQueryResponse(
                    responseJson,
                    merchantOrderNo,
                    amount
                );
            }
            catch (Exception ex)
            {
                return Result<NewebPayQueryResultViewModel>.Fail(
                    ErrorCodes.SystemError,
                    "呼叫藍新單筆交易查詢 API 失敗：" + ex.Message
                );
            }
        }
        /// <summary>
        /// 處理藍新單筆交易查詢回傳 JSON
        /// </summary>
        public Result<NewebPayQueryResultViewModel> HandleQueryResponse(
            string responseJson,
            string expectedMerchantOrderNo,
            int expectedAmount)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    return Result<NewebPayQueryResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新單筆交易查詢回傳內容為空"
                    );
                }

                var json = JObject.Parse(responseJson);

                var status = json["Status"]?.ToString();
                var message = json["Message"]?.ToString();

                if (status != "SUCCESS")
                {
                    return Result<NewebPayQueryResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新單筆交易查詢失敗：" + message
                    );
                }

                var result = json["Result"] as JObject;

                if (result == null)
                {
                    return Result<NewebPayQueryResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新單筆交易查詢 Result 為空"
                    );
                }

                var merchantOrderNo = result["MerchantOrderNo"]?.ToString();
                var amtText = result["Amt"]?.ToString();

                if (merchantOrderNo != expectedMerchantOrderNo)
                {
                    return Result<NewebPayQueryResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳訂單編號不一致"
                    );
                }

                int responseAmount;

                if (!int.TryParse(amtText, out responseAmount))
                {
                    return Result<NewebPayQueryResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳金額格式錯誤"
                    );
                }

                if (responseAmount != expectedAmount)
                {
                    return Result<NewebPayQueryResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳金額不一致"
                    );
                }
                var vm = new NewebPayQueryResultViewModel
                {
                    RawJson = json.ToString(),

                    Status = status,
                    Message = message,

                    MerchantOrderNo = merchantOrderNo,
                    Amt = amtText,
                    TradeNo = result["TradeNo"]?.ToString(),
                    TradeStatus = result["TradeStatus"]?.ToString(),
                    CloseStatus = result["CloseStatus"]?.ToString(),
                    BackStatus = result["BackStatus"]?.ToString(),
                    BackBalance = result["BackBalance"]?.ToString()
                };

                return Result<NewebPayQueryResultViewModel>.Success(
                    vm,
                    "藍新單筆交易查詢成功"
                );
            }
            catch (Exception ex)
            {
                return Result<NewebPayQueryResultViewModel>.Fail(
                    ErrorCodes.SystemError,
                    "處理藍新單筆交易查詢回傳失敗：" + ex.Message
                );
            }
        }
        //藍新請退款API
        public Result<NewebPayCloseResultViewModel> CloseTrade(
            int orderId,int closeType)
        {
            try
            {
                var order = _orderRepository.GetEntityById(orderId);

                if (order == null)
                {
                    return Result<NewebPayCloseResultViewModel>.Fail(
                        ErrorCodes.NotFound,
                        "找不到訂單"
                    );
                }

                var payment = _paymentRepository.GetOrderId(orderId);

                if (payment == null)
                {
                    return Result<NewebPayCloseResultViewModel>.Fail(
                        ErrorCodes.NotFound,
                        "找不到付款資料"
                    );
                }

                if (payment.PaymentMethod != PaymentMethodName.CreditCard)
                {
                    return Result<NewebPayCloseResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "此訂單不是信用卡付款"
                    );
                }
                //從 Web.config 讀藍新的設定                
                var merchantId = AppConfigHelper.GetRequiredSetting(
                               "COS_NEWEBPAY_MERCHANT_ID");
                var hashKey = AppConfigHelper.GetRequiredSetting(
                              "COS_NEWEBPAY_HASH_KEY");
                var hashIV = AppConfigHelper.GetRequiredSetting(
                            "COS_NEWEBPAY_HASH_IV");
                var queryUrl = ConfigurationManager.AppSettings["NewebPay.CreditCardCloseUrl"];

                if (string.IsNullOrWhiteSpace(queryUrl))
                {
                    queryUrl = "https://ccore.newebpay.com/API/CreditCard/Close";
                }

                if (string.IsNullOrWhiteSpace(merchantId) ||
                    string.IsNullOrWhiteSpace(hashKey) ||
                    string.IsNullOrWhiteSpace(hashIV))
                {
                    return Result<NewebPayCloseResultViewModel>.Fail(
                        ErrorCodes.SystemError,
                        "藍新金流設定不完整"
                    );
                }
                //組出組PostData需要的參數
                // 藍新金額要純整數
                var amount = Convert.ToInt32(payment.Amount);
                var amountText = amount.ToString();
                var merchantOrderNo = order.OrderNo;
                var postDataParams = new Dictionary<string, string>
                {
                    {"RespondType", "JSON"},
                    { "Version", "1.1" },
                    {"Amt",amountText },
                    {"MerchantOrderNo",order.OrderNo },
                    {"TimeStamp" ,NewebPayCryptoHelper.GetUnixTimestamp()},
                    {"IndexType", "1" },
                    { "CloseType", closeType.ToString() }
                };
                //變成字串
                var postDataRaw =string.Join("&",
                    postDataParams.Select(x => $"{HttpUtility.UrlEncode(x.Key)}={HttpUtility.UrlEncode(x.Value)}"));
                //aes加密
                var postData = NewebPayCryptoHelper.EncryptTradeInfo(
                    postDataRaw, hashKey, hashIV);
               
                var requestDto = new NewebPayCloseRequestDto
                {
                    MerchantID=merchantId,
                    PostData=postData,
                };

                //DTO轉FormData
                var formData = new Dictionary<string, string>
            {
                { "MerchantID_", requestDto.MerchantID },
                { "PostData_", requestDto.PostData },
                
            };

                string responseJson;

                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(formData);

                    var httpResponse = client
                        .PostAsync(queryUrl, content)
                        .GetAwaiter()
                        .GetResult();

                    responseJson = httpResponse.Content
                        .ReadAsStringAsync()
                        .GetAwaiter()
                        .GetResult();

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        return Result<NewebPayCloseResultViewModel>.Fail(
                            ErrorCodes.SystemError,
                            "藍新單筆交易查詢 HTTP 失敗：" + responseJson
                        );
                    }
                    
                }
                // 丟給接收 Service 處理 JSON
                return HandleCloseResponse(
                    responseJson, merchantOrderNo,
                    amount);
            }
            catch (Exception ex) 
            {
                return Result<NewebPayCloseResultViewModel>.Fail(
                    ErrorCodes.SystemError,
                    "呼叫藍新請退款 API 失敗：" + ex.Message
                );
            }

        }
        //處理藍新請退款回傳的 JSON
        public Result<NewebPayCloseResultViewModel> HandleCloseResponse( string responseJson,string expectedMerchantOrderNo,
            int expectedAmount)
        {
            try 
            {
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    return Result<NewebPayCloseResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新單筆交易查詢回傳內容為空"
                    );
                }

                var json = JObject.Parse(responseJson);

                var status = json["Status"]?.ToString();
                var message = json["Message"]?.ToString();

                if (status != "SUCCESS")
                {
                    return Result<NewebPayCloseResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新請退款失敗：" + message
                    );
                }

                var result = json["Result"] as JObject;

                if (result == null)
                {
                    return Result<NewebPayCloseResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新請退款 Result 為空"
                    );
                }

                var merchantOrderNo = result["MerchantOrderNo"]?.ToString();
                var amtText = result["Amt"]?.ToString();

                if (merchantOrderNo != expectedMerchantOrderNo)
                {
                    return Result<NewebPayCloseResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳訂單編號不一致"
                    );
                }

                int responseAmount;

                if (!int.TryParse(amtText, out responseAmount))
                {
                    return Result<NewebPayCloseResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳金額格式錯誤"
                    );
                }

                if (responseAmount != expectedAmount)
                {
                    return Result<NewebPayCloseResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳金額不一致"
                    );
                }
       
        
            var vm = new NewebPayCloseResultViewModel
            {
                    RawJson = json.ToString(),
                    Status = status,
                    Message = message,
                    MerchantOrderNo = merchantOrderNo,                   
                    TradeNo = result["TradeNo"]?.ToString(),
                    Amt= responseAmount,
                MerchantID = result["MerchantID"]?.ToString()


            };

                return Result<NewebPayCloseResultViewModel>.Success(
                    vm,
                    "藍新請退款成功"
                );
            }
            catch (Exception ex) 
            {
                return Result<NewebPayCloseResultViewModel>.Fail(
                    ErrorCodes.SystemError,
                    "處理藍新請退款回傳失敗：" + ex.Message
                );
            }
        }
        //取消授權api
        public Result<NewebPayCancelAuthorizationResultViewModel>CancelCreditCardAuthorization(int orderId)
        {
            try 
            {
                var order =_orderRepository.GetEntityById(orderId);
                if (order == null)
                {
                    return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                     ErrorCodes.NotFound,
                     "找不到訂單"
                        );
                }
                var payment =_paymentRepository.GetOrderId(orderId);
                if (payment == null) 
                {
                    return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                ErrorCodes.NotFound,
                "找不到付款資料"
                        );
                }
                if (payment.PaymentMethod != PaymentMethodName.CreditCard)
                {
                    return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "此訂單不是信用卡付款"
                    );
                }

                if (payment.PaymentStatusId != PaymentStatusIds.Paid)
                {
                    return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "只有已授權成功的信用卡付款可以取消授權"
                    );
                }

                if (payment.IsCaptured)
                {
                    return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "此訂單已請款，不能取消授權，請改走退款"
                    );
                }
                var merchantId =AppConfigHelper.GetRequiredSetting("COS_NEWEBPAY_MERCHANT_ID");
                var hashKey = AppConfigHelper.GetRequiredSetting("COS_NEWEBPAY_HASH_KEY");
                var hashIV = AppConfigHelper.GetRequiredSetting
                    ("COS_NEWEBPAY_HASH_IV");
                var cancelUrl = ConfigurationManager.AppSettings["NewebPay.CreditCardCancelUrl"];
                var amount = Convert.ToInt32(payment.Amount);
                var amountText = amount.ToString();
                var postDataParams = new Dictionary<string, string>
                 {
                          { "RespondType", "JSON" },
                         { "Version", "1.0" },
                         { "Amt", amountText },
                         { "MerchantOrderNo", order.OrderNo },
                          { "IndexType", "1" },
                          { "TimeStamp", NewebPayCryptoHelper.GetUnixTimestamp() }
                };
                var postDataRaw = BuildQueryString(postDataParams);
                var postData = NewebPayCryptoHelper.EncryptTradeInfo(
                                         postDataRaw,
                                         hashKey,
                                        hashIV);
                var formData = new Dictionary<string, string>
                 {
                          { "MerchantID_", merchantId },
                             { "PostData_", postData }
                  };
                string responseText;
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(formData);

                    var httpResponse = client
                        .PostAsync(cancelUrl, content)
                        .GetAwaiter()
                        .GetResult();

                    responseText = httpResponse.Content
                        .ReadAsStringAsync()
                        .GetAwaiter()
                        .GetResult();

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                            ErrorCodes.SystemError,
                            "藍新取消授權 HTTP 失敗：" + responseText
                        );
                    }
                }
                var handleResult = HandleCancelAuthorizationResponse(
                responseText,
                order.OrderNo,
                amount);
                if (!handleResult.IsSuccess)
                {
                    return handleResult;
                }

                var vm = handleResult.Data;

                if (vm.Status == "SUCCESS")
                {
                    var errorMessage = payment.MarkAuthorizationCancelled(
                        vm.Amt,
                        vm.TradeNo,
                        vm.RawResponse,
                        vm.Message
                    );

                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                            ErrorCodes.Validation,
                            errorMessage
                        );
                    }

                    _paymentRepository.SaveChanges();

                    return Result<NewebPayCancelAuthorizationResultViewModel>.Success(
                        vm,
                        "藍新取消授權成功"
                    );
                }
                return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
            ErrorCodes.Validation,
            "藍新取消授權失敗：" + vm.Message);
            }
            catch (Exception ex) 
            {
                return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
            ErrorCodes.SystemError,
            "呼叫藍新取消授權 API 失敗：" + ex.Message
                );
            }
        }
        // 處理藍新取消授權回傳的 JSON
        public Result<NewebPayCancelAuthorizationResultViewModel> HandleCancelAuthorizationResponse(
    string responseText,
    string expectedMerchantOrderNo,
    int expectedAmount)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(responseText))
                {
                    return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新取消授權回傳內容為空"
                    );
                }

                var json = JObject.Parse(responseText);

                var status = json["Status"]?.ToString();
                var message = json["Message"]?.ToString();

                var result = json["Result"] as JObject;

                if (result == null)
                {
                    return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新取消授權 Result 為空"
                    );
                }

                var merchantOrderNo = result["MerchantOrderNo"]?.ToString();
                var amtText = result["Amt"]?.ToString();

                if (merchantOrderNo != expectedMerchantOrderNo)
                {
                    return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳訂單編號不一致"
                    );
                }

                int responseAmount;

                if (!int.TryParse(amtText, out responseAmount))
                {
                    return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳金額格式錯誤"
                    );
                }

                if (responseAmount != expectedAmount)
                {
                    return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                        ErrorCodes.Validation,
                        "藍新回傳金額不一致"
                    );
                }

                var vm = new NewebPayCancelAuthorizationResultViewModel
                {
                    RawResponse = responseText,
                    Status = status,
                    Message = message,
                    MerchantID = result["MerchantID"]?.ToString(),
                    MerchantOrderNo = merchantOrderNo,
                    TradeNo = result["TradeNo"]?.ToString(),
                    Amt = responseAmount,
                    CheckCode = result["CheckCode"]?.ToString()
                };

                return Result<NewebPayCancelAuthorizationResultViewModel>.Success(
                    vm,
                    "藍新取消授權回應處理成功"
                );
            }
            catch (Exception ex)
            {
                return Result<NewebPayCancelAuthorizationResultViewModel>.Fail(
                    ErrorCodes.SystemError,
                    "處理藍新取消授權回傳失敗：" + ex.Message
                );
            }
        }


    }

}
