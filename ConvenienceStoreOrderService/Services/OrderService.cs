using ConvenienceStoreOrderService.Services.Interfaces;
using ConvenienceStoreOrderService.Models.ViewModels;
using ConvenienceStoreOrderService.Repositories;
using ConvenienceStoreOrderService.Repositories.Interfaces;
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

namespace ConvenienceStoreOrderService.Services
{
    public class OrderService : IOrderService
    {
        private IOrderRepository _orderRepository;
        private IOrderDetailRepository _orderDetailRepository;
        private IOrderStatusService _orderStatusService;
        private readonly AppDbContext _db;
        private IProductRepository _productRepository;
        private IPaymentStatusService _paymentStatusService;
        private IPaymentRepository _paymentRepository;
        private IPaymentService _paymentService;
        private IPaymentStatusRepository _paymentStatusRepository;
        private IShipmentRepository _shipmentRepository;
        public OrderService(IOrderRepository orderRepository, IOrderStatusService orderStatusService,AppDbContext db, IProductRepository productRepository, IPaymentStatusService paymentStatusService,IPaymentService paymentService,IOrderDetailRepository orderDetailRepository, IPaymentRepository paymentRepository,IPaymentStatusRepository paymentStatusRepository,IShipmentRepository shipmentRepository)
        {
            _orderRepository = orderRepository;
            _orderStatusService = orderStatusService;
            _db = db;
            _productRepository = productRepository;
            _paymentStatusService = paymentStatusService;
            _orderDetailRepository = orderDetailRepository;
            _paymentRepository = paymentRepository;
            _paymentService = paymentService;
            _paymentStatusRepository= paymentStatusRepository;
            _shipmentRepository = shipmentRepository;



        }
        public List<OrderViewModel> GetOrders()
        {
            var dtoList= _orderRepository.GetOrderListForDisplay();
            return dtoList.Select(o=> OrderMapper.ToVM(o)).ToList();
        }
        //Processing->ReadyToShip
        public Result<bool> MarkReadyToShip (int orderId)
        {
            var order =_orderRepository.GetEntityById(orderId);
            if(order == null)
            { return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單"); }

            // 查目前訂單狀態
            var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
            if(!currentStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(
            ErrorCodes.SystemError,
            "查詢目前訂單狀態失敗。");
            }

            //  查「待出貨」的資料
            var targetStatusResult = _orderStatusService.GetByCode("ReadyToShip");
            if(!targetStatusResult.IsSuccess) 
            {
                return Result<bool>.Fail(
            ErrorCodes.SystemError,
            "找不到已出貨狀態設定。");
            }

            //把目前狀態 Code Id 丟給 Orders 自己判斷
            var errorMessage = order.MarkReadyToShip(
                targetStatusResult.Data.OrderStatusId,
                currentStatusResult.Data.OrderStatusCode
        
    );
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
            }
            _orderRepository.SaveChanges();
            return Result<bool>.Success(true, "訂單狀態已更新為待出貨。");

        }
        //ReadyToShip->Shipped，扣庫存
        public Result<bool> MarkShipped (int orderId) 
        {
            var tran = _db.Database.BeginTransaction();
            try
            {
                var order = _orderRepository.GetEntityById(orderId);
                if (order == null)
                { return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單"); }
                //查Order的OrderStatusId目前訂單狀態
                var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
                if (!currentStatusResult.IsSuccess)
                {
                    return Result<bool>.Fail(
                ErrorCodes.SystemError,
                "查詢目前訂單狀態失敗。");
                }
                //查「已出貨」
                var targetStatusResult = _orderStatusService.GetByCode("Shipped");
                if (!targetStatusResult.IsSuccess)
                {
                    return Result<bool>.Fail(
                        ErrorCodes.SystemError,
                        "找不到已出貨狀態。"
                    );
                }
                //線上付款Shipped 前必須 Paid
                var paymentCheckResult = _paymentService.CheckCanShip(orderId);
                if (!paymentCheckResult.IsSuccess)
                {
                    tran.Rollback();
                    return paymentCheckResult;
                }
                //扣庫存
                var stockResult = StockWhenShipped(orderId);
                if (!stockResult.IsSuccess)
                {
                    tran.Rollback();
                    return stockResult;
                }
                //把orderstatusId丟給order判斷
                var errorMessage = order.MarkShipped
                    (targetStatusResult.Data.OrderStatusId,
                    currentStatusResult.Data.OrderStatusCode
                    );
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
                }
                _orderRepository.SaveChanges();
                tran.Commit();
                return Result<bool>.Success(true, "訂單狀態已更新為已出貨。");
            }
            catch (Exception ex) 
            {
                tran.Rollback();
                return Result<bool>.Fail(ErrorCodes.Validation, "無法出貨，請稍後再試");
            }
            finally { tran.Dispose(); }
        }
        //Shipped->Arrived
        public Result<bool> MarkArrived(int orderId)
        {
            var order = _orderRepository.GetEntityById(orderId);
            if(order == null)
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");
            }
            //查Order的OrderStatusId目前訂單狀態
            var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
            if(!currentStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError,
            "查詢目前訂單狀態失敗。");
            }
            //查要改的StatusCode「已到店」狀態
            var targetStatusResult = _orderStatusService.GetByCode("Arrived");
            if (!targetStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError,
                    "找不到已到店狀態設定。");
            }
            //改orderstatusId
            var result = order.MarkArrived
                (
                 targetStatusResult.Data.OrderStatusId,
                currentStatusResult.Data.OrderStatusCode

                );
            if (!string.IsNullOrEmpty(result))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, result);
            }
            _orderRepository.SaveChanges();
            return Result<bool>.Success(true, "訂單狀態已更新為已到店。");
        }
        //Arrived->PickedUp，COD取貨付款
        public Result<bool> MarkPickedUp(int orderId)
        {
            var tran = _db.Database.BeginTransaction();
            try
            {
                var order = _orderRepository.GetEntityById(orderId);
                if (order == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");
                }
                //查Order的OrderStatusId目前訂單狀態
                var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
                if (!currentStatusResult.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError,
                "查詢目前訂單狀態失敗。");
                }
                //查要改的StatusCode「已取貨」狀態
                var targetStatusResult = _orderStatusService.GetByCode("PickedUp");
                if (!targetStatusResult.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError,
                        "找不到已取貨狀態。");
                }
                //改orderstatusId
                var result = order.MarkPickedUp
                    (
                     targetStatusResult.Data.OrderStatusId,
                    currentStatusResult.Data.OrderStatusCode

                    );
                if (!string.IsNullOrEmpty(result))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Conflict, result);
                }
                //付款狀態
                var paymentResult = _paymentService.MarkCodPaidWhenPickedUp(orderId);

                if (!paymentResult.IsSuccess)
                {
                    tran.Rollback();
                    return paymentResult;
                }
                _orderRepository.SaveChanges();
                tran.Commit();
                return Result<bool>.Success(true, "訂單狀態已更新為已取貨。");
            }
            catch (Exception ex) 
            { 
                tran.Rollback();
                return Result<bool>.Fail(ErrorCodes.SystemError, "取貨失敗，請稍後再試");
            }
            finally { tran.Dispose(); }
        }
        //取消訂單
        public Result<bool> CancelOrder (int orderId,string cancelReson) 
        {
            var tran =_db.Database.BeginTransaction();
            try 
            {
                //取消原因必填
                if (string.IsNullOrWhiteSpace(cancelReson))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "取消原因必填");
                    
                }
                //找orderId
                var order = _orderRepository.GetEntityById(orderId);
                if (order == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");
                }
                //找order.statusId的code、name
                var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
                if (!currentStatusResult.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError, "找不到物流狀態");
                }
                //找Cancelled對應的id、name
                var targetStatusResult = _orderStatusService.GetByCode("Cancelled");
                if (!targetStatusResult.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError, "找不到取消訂單");
                }
                //把code、id丟給ORDER.CS判斷，不是就回錯誤訊息
                var errorMessage = order.CancelOrder(
                    targetStatusResult.Data.OrderStatusId, currentStatusResult.Data.OrderStatusCode);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
                }

                // 處理付款、退款
                var paymentResult =_paymentService.HandleCancelPayment(orderId,cancelReson);
                if (!paymentResult.IsSuccess) 
                {
                    tran.Rollback();
                    return paymentResult;
                }
                //釋放預留庫存
                var releaseReault = ReleaseReservedStock(orderId);
                if (!releaseReault.IsSuccess)
                {
                    tran.Rollback();
                    return releaseReault;
                }
                
                order.CancelReason = cancelReson;
                _orderRepository.SaveChanges();
                tran.Commit();
                return Result<bool>.Success(true, "訂單已取消，已釋放預留庫存");
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return Result<bool>.Fail(ErrorCodes.SystemError, "取消訂單失敗，請稍後再試");
            }
            finally
            {
                tran.Dispose();
            }  
        }
        //建立訂單
        public Result<int> PlaceOrder(PlaceOrderDto dto)
        {
            if (dto == null) 
            {
                return Result<int>.Fail(ErrorCodes.Validation, "下單資料不可為空");
            }
            if(dto.Quantity <=0)
            {
                return Result<int>.Fail(ErrorCodes.Validation, "購買數量必須大於 0");
            }
            int maxRetry = 3;//最多嘗試下單三次
            var tran=_db.Database.BeginTransaction();
            for (int attempt = 1; attempt <= maxRetry; attempt++)
            {
                try
                {
                    //下單流程
                    var now = DateTime.Now;
                    const int shippingFee = 60;
                    // 查商品
                    var product = _productRepository.GetEntityById(dto.ProductId);
                    if (product == null)
                    {
                        return Result<int>.Fail(ErrorCodes.NotFound, "找不到商品");
                    }

                    if (!product.IsActive)
                    {
                        return Result<int>.Fail(ErrorCodes.Validation, "商品未上架");
                    }
                    // 檢查可賣庫存
                    var stock = product.StockOnHand - product.StockReserved;
                    if (stock < dto.Quantity)
                    {
                        return Result<int>.Fail(ErrorCodes.Conflict, "庫存不足");
                    }
                    // 預留庫存
                    product.StockReserved += dto.Quantity;
                    // ProductVersion+1
                    product.ProductVersion += 1;
                    // 計算金額
                    var unitPrice = product.Price;
                    var subTotal = unitPrice * dto.Quantity;
                    var orderTotal = subTotal + shippingFee;
                    // 取得訂單狀態 Processing
                    var orderStatusResult = _orderStatusService.GetByCode("Processing");
                    if (!orderStatusResult.IsSuccess)
                    {
                        return Result<int>.Fail(ErrorCodes.SystemError, "找不到訂單狀態");
                    }
                    // 取得付款狀態 Pending
                    var paymentStatusResult = _paymentStatusService.GetByCode("Pending");
                    if (!paymentStatusResult.IsSuccess)
                    {
                        return Result<int>.Fail(ErrorCodes.SystemError, "找不到付款狀態");
                    }
                    // 建立 Order
                    var order = new Order
                    {
                        OrderNo = CreateOrderNo(),
                        BuyerUserId = dto.BuyerUserId,
                        SellerUserId = dto.SellerUserId,
                        OrderSource = 1,
                        CreatedAt = now,
                        ShippingFee = shippingFee,
                        OrderTotal = orderTotal,
                        //PaymentDueAt = now.AddMinutes(15)
                        PaymentDueAt = null,
                        InfoDueAt = now.AddMinutes(15),
                        InfoCompletedAt = null
                    };
                    // 狀態初始化走 Order 封裝
                    order.InitProcessing(orderStatusResult.Data.OrderStatusId);
                    // 產生OrderId 
                    _orderRepository.Add(order);
                    _orderRepository.SaveChanges();
                    // 建立 OrderDetail
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        UnitPrice = unitPrice,
                        Quantity = dto.Quantity,
                        SubTotal = subTotal

                    };
                    _orderDetailRepository.Add(orderDetail);

                    // 建立 Payment

                    var payment = new Payment
                    {
                        OrderId = order.OrderId,
                        TradeNo = CreateTradeNo(),
                        Amount = orderTotal,
                        PaidAt = null,
                        RawCallBack = null,
                        CreatedAt = now,
                        PaymentProvider = "測試",
                        PaymentMethod = dto.PaymentMethod,
                        RefundStatusId = RefundStatusIds.None


                    };
                    payment.InitPending(paymentStatusResult.Data.PaymentStatusId);
                    _paymentRepository.Add(payment);
                    // 存 OrderDetail + Payment + Product.StockReserved
                    _orderRepository.SaveChanges();
                    tran.Commit();
                    return Result<int>.Success(order.OrderId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    tran.Rollback();
                    
                    // 清掉 EF 追蹤狀態
                    foreach (var entry in _db.ChangeTracker.Entries())
                    {
                        entry.State = System.Data.Entity.EntityState.Detached;
                    }

                    if (attempt == maxRetry)
                    {
                        return Result<int>.Fail(
                            ErrorCodes.Conflict,
                            "商品庫存正在被其他訂單更新，請重新下單。");
                    }

                    
                    continue;
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    return Result<int>.Fail(
                        ErrorCodes.SystemError,
                        "下單失敗：" + ex.Message);
                }
                finally
                {
                    tran.Dispose();
                }
                
            }
            return Result<int>.Fail(ErrorCodes.SystemError, "下單失敗");
        }
        //物流退貨，建立退款申請
        public Result<bool> ShipmentReturned(int orderId,string returnReson)
        {
            var tran =_db.Database.BeginTransaction();
            try 
            {
                //找訂單
                var order = _orderRepository.GetEntityById(orderId);

                if (order == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.NotFound, "找不到訂單");
                }
                // 找目前訂單狀態
                var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);

                if (!currentStatusResult.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError, "找不到目前訂單狀態");
                }
                // 找 Returned 狀態
                var targetStatusResult = _orderStatusService.GetByCode("Returned");

                if (!targetStatusResult.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError, "找不到退回訂單狀態");
                }
                // 給 Order判斷
                var errorMessage = order.MarkReturned(
                    targetStatusResult.Data.OrderStatusId,
                    currentStatusResult.Data.OrderStatusCode
                );
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
                }
                // 找物流
                var shipment = _shipmentRepository.GetByOrderId(orderId);
                if (shipment == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.NotFound, "找不到物流資料");
                }
                // ShipmentStatus 改 Returned、UpdatedAt
                var shipmentStatus = shipment.ShipmentStatusId = ShipmentStatusIds.Returned;
                shipment.UpdatedAt = DateTime.Now;
                // 物流退回：回補 OnHand，Reserved 不動
                var restoreResult = AddStockOnHand(orderId);
                if (!restoreResult.IsSuccess)
                {
                    tran.Rollback();
                    return restoreResult;
                }
                // 處理付款狀態               
                var paymentResult = _paymentService.RequestRefund(orderId,returnReson);

                if (!paymentResult.IsSuccess)
                {
                    tran.Rollback();
                    return paymentResult;
                }

                
                _orderRepository.SaveChanges();

                tran.Commit();

                return Result<bool>.Success(true, "訂單已退回，物流已退回，庫存已回補，已建立退款申請");


            }
            catch (Exception ex) 
            { tran.Rollback();
                return Result<bool>.Fail(ErrorCodes.SystemError, "退回訂單失敗，請稍後再試");
            }
            finally { tran.Dispose(); }
            
        }
        //訂單編號
        private static readonly Random _random = new Random();
        public string CreateOrderNo()
        {
            return "ORD" + DateTime.Now.ToString("MMdd") + _random.Next(100, 999);
        }
        //交易編號
        public string CreateTradeNo()
        {
            return "TRADE"
                + DateTime.Now.ToString("yyyyMMddHHmmss")
                + new Random().Next(1000, 9999);
        }
        //扣庫存
        public Result<bool> StockWhenShipped(int orderId)
        {
            var orderDetail =_orderRepository.GetOrderDetailId(orderId);
            if (orderDetail == null || !orderDetail.Any())
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到訂單明細");
            }
               
            foreach (var item in orderDetail)
            {
                var product =_productRepository.GetEntityById(item.ProductId);
                if (product == null)
                {
                    return Result<bool>.Fail(ErrorCodes.NotFound, "找不到商品資料");
                }
                //檢查預留庫存
                if(product.StockReserved < item.Quantity)
                {
                    return Result<bool>.Fail(ErrorCodes.Validation, "預留庫存不足，無法釋放");
                }
                //檢查實際庫存
                if(product.StockOnHand < item.Quantity)
                {
                    return Result<bool>.Fail(ErrorCodes.Validation, "實際庫存不足，無法釋放");
                }
                product.StockReserved -= item.Quantity;
                product.StockOnHand -= item.Quantity;
            }
            return Result<bool>.Success(true);
        }
        //釋放預留庫存StockOnHand不動，StockReserved-=qty
        public Result<bool> ReleaseReservedStock(int orderId)
        {
            var orderDetails = _orderRepository.GetOrderDetailId(orderId);

            if (orderDetails == null || !orderDetails.Any())
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到訂單明細");
            }

            foreach (var item in orderDetails)
            {
                var product = _productRepository.GetEntityById(item.ProductId);

                if (product == null)
                {
                    return Result<bool>.Fail(ErrorCodes.NotFound, "找不到商品資料");
                }

                if (product.StockReserved < item.Quantity)
                {
                    return Result<bool>.Fail(ErrorCodes.Validation, "預留庫存不足，無法釋放");
                }

                product.StockReserved -= item.Quantity;
            }

            return Result<bool>.Success(true);
        }
        //回補實際庫存，StockOnHand+=qty，StockReserved不動
        private Result<bool> AddStockOnHand(int orderId)
        {
            var orderDetails = _orderRepository.GetOrderDetailId(orderId);

            if (orderDetails == null || !orderDetails.Any())
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到訂單明細");
            }

            foreach (var item in orderDetails)
            {
                var product = _productRepository.GetEntityById(item.ProductId);

                if (product == null)
                {
                    return Result<bool>.Fail(ErrorCodes.NotFound, "找不到商品資料");
                }

                
                product.StockOnHand += item.Quantity;
            }

            return Result<bool>.Success(true);
        }
        //信用卡填完物流資料、準備導藍新時倒數
        public Result<bool> StartPaymentCountdown(int orderId)
        {
            var order = _orderRepository.GetEntityById(orderId);

            if (order == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到訂單");
            }

            order.PaymentDueAt = DateTime.Now.AddMinutes(15);

            _orderRepository.SaveChanges();

            return Result<bool>.Success(true);
        }
        //逾時未付款自動取消訂單

        public Result<int> AutoCancelUnpaidOrders()
        {
            
            var now=DateTime.Now;
            var expiredOrderIds = _orderRepository.GetUnpaidOrderIds(now);
            
            int count = 0;
            List<string> errors = new List<string>();
            foreach (var orderId in expiredOrderIds) 
            {
                var result = CancelExpiredUnpaidOrder(orderId);
                if(result.IsSuccess)
                {
                    count++;
                }
                else
                {
                    errors.Add($"OrderId {orderId}：{result.Message}");
                }
            }
            if (errors.Any())
            {
                return Result<int>.Fail(
                    ErrorCodes.Validation,
                    "有訂單取消失敗：" + string.Join("；", errors)
                );
            }
            return Result<int>.Success(count);
            
        }

        //取消單筆逾期未付款訂單
        public Result<bool> CancelExpiredUnpaidOrder(int orderId)
        {
            var tran = _db.Database.BeginTransaction();
            try
            {
                var order = _orderRepository.GetEntityById(orderId);
                if (order == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");
                }
                var payment=_paymentRepository.GetOrderId(orderId);
                if(payment == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "找不到付款資料");
                }
                var orderDetails = _orderDetailRepository.GetOrderDetails(orderId);
                if (orderDetails == null || !orderDetails.Any())
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單明細");
                }
                // 再檢查一次，避免排程掃到後，使用者剛好已付款
                var paymentStatus =_paymentStatusRepository.GetById(payment.PaymentStatusId);
                if (paymentStatus.PaymentStatusCode != "Pending")
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "付款狀態已不是 Pending，不可自動取消");
                }
                var orderStatus = _orderStatusService.GetById(order.OrderStatusId);
                if (!orderStatus.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError, "查詢訂單狀態失敗");
                }
                if (orderStatus.Data.OrderStatusCode != "Processing")
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "訂單狀態已不是 Processing，不可自動取消");
                }
                //取消訂單
                var cancelOrderResult = _orderStatusService.GetByCode("Cancelled");
                if(!cancelOrderResult.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError, "找不到 Cancelled 訂單狀態");
                }
                var cancelError = order.CancelOrder(cancelOrderResult.Data.OrderStatusId,
                    orderStatus.Data.OrderStatusCode 
                   );
                if(!string.IsNullOrEmpty(cancelError)) 
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, cancelError);
                }
                order.CancelReason = "線上付款逾時未付款，系統自動取消";
                // Payment 改 Cancelled
                var cancelledPaymentStatus = _paymentStatusRepository.GetByCode("Cancelled");
                if (cancelledPaymentStatus == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError, "找不到 Cancelled 付款狀態");
                }
                var paymentError = payment.CancelPending(); 
                if (!string.IsNullOrEmpty(paymentError))
                {
                    tran.Rollback();

                    return Result<bool>.Fail(ErrorCodes.Validation, paymentError);
                }
                // 釋放 Reserved
                var releaseResult = ReleaseReservedStock(orderId);

                if (!releaseResult.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(releaseResult.ErrorCode, releaseResult.Message);
                }
                _orderRepository.SaveChanges();
                tran.Commit();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return Result<bool>.Fail(ErrorCodes.SystemError, ex.Message);
            }
            finally { tran.Dispose(); }
        }
        //物流資料填寫完成時間
        public Result<bool> MarkInfoCompleted(int orderId)
        {
            var order = _orderRepository.GetEntityById(orderId);

            if (order == null)
            {
                return Result<bool>.Fail(ErrorCodes.NotFound, "找不到訂單");
            }

            order.InfoCompletedAt = DateTime.Now;

            _orderRepository.SaveChanges();

            return Result<bool>.Success(true);
        }
        //逾時物流資料未填寫自動取消訂單
        public Result<int> AutoCancelIncompleteOrders()
        {
            var now = DateTime.Now;
            var expiredOrderIds = _orderRepository.GetIncompleteOrderIds(now);

            int count = 0;
            List<string> errors = new List<string>();
            foreach (var orderId in expiredOrderIds)
            {
                var result = CancelExpiredIncompleteOrder(orderId);
                if (result.IsSuccess)
                {
                    count++;
                }
                else
                {
                    errors.Add($"OrderId {orderId}：{result.Message}");
                }
            }
            if (errors.Any())
            {
                return Result<int>.Fail(
                    ErrorCodes.Validation,
                    "有訂單取消失敗：" + string.Join("；", errors)
                );
            }
            return Result<int>.Success(count);
        }
        //取消單筆逾期物流資料未填寫訂單
        public Result<bool> CancelExpiredIncompleteOrder(int orderId)
        {
            var tran = _db.Database.BeginTransaction();
            try
            {
                var order = _orderRepository.GetEntityById(orderId);
                if (order == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");
                }
                var payment = _paymentRepository.GetOrderId(orderId);
                if (payment == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "找不到付款資料");
                }
                var orderDetails = _orderDetailRepository.GetOrderDetails(orderId);
                if (orderDetails == null || !orderDetails.Any())
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單明細");
                }
                // 再檢查一次，避免排程掃到後，使用者剛好已付款
                var paymentStatus = _paymentStatusRepository.GetById(payment.PaymentStatusId);
                if (paymentStatus.PaymentStatusCode != "Pending")
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "付款狀態已不是 Pending，不可自動取消");
                }
                var orderStatus = _orderStatusService.GetById(order.OrderStatusId);
                if (!orderStatus.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError, "查詢訂單狀態失敗");
                }
                if (orderStatus.Data.OrderStatusCode != "Processing")
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "訂單狀態已不是 Processing，不可自動取消");
                }
                
                //判斷物流資料填寫時間
                var now = DateTime.Now;
                if (order.InfoCompletedAt != null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "訂單資料已完成，不可自動取消");
                }

                if (order.InfoDueAt == null || order.InfoDueAt >= now)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, "訂單資料尚未逾時，不可自動取消");
                }
                
                order.CancelReason = "訂單資料逾時未完成，系統自動取消";
                
                //取消訂單
                var cancelOrderResult = _orderStatusService.GetByCode("Cancelled");
                if (!cancelOrderResult.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError, "找不到 Cancelled 訂單狀態");
                }
                var cancelError = order.CancelOrder(cancelOrderResult.Data.OrderStatusId,
                    orderStatus.Data.OrderStatusCode
                   );
                if (!string.IsNullOrEmpty(cancelError))
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.Validation, cancelError);
                }
                // Payment 改 Cancelled
                var cancelledPaymentStatus = _paymentStatusRepository.GetByCode("Cancelled");
                if (cancelledPaymentStatus == null)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(ErrorCodes.SystemError, "找不到 Cancelled 付款狀態");
                }
                var paymentError = payment.CancelPending();
                if (!string.IsNullOrEmpty(paymentError))
                {
                    tran.Rollback();

                    return Result<bool>.Fail(ErrorCodes.Validation, paymentError);
                }
                
                // 釋放 Reserved
                var releaseResult = ReleaseReservedStock(orderId);

                if (!releaseResult.IsSuccess)
                {
                    tran.Rollback();
                    return Result<bool>.Fail(releaseResult.ErrorCode, releaseResult.Message);
                }
                _orderRepository.SaveChanges();
                tran.Commit();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return Result<bool>.Fail(ErrorCodes.SystemError, ex.Message);
            }
            finally { tran.Dispose(); }
        }

    }
}