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
        public OrderService(IOrderRepository orderRepository, IOrderStatusService orderStatusService,AppDbContext db, IProductRepository productRepository, IPaymentStatusService paymentStatusService, IOrderDetailRepository orderDetailRepository, IPaymentRepository paymentRepository)
        {
            _orderRepository = orderRepository;
            _orderStatusService = orderStatusService;
            _db = db;
            _productRepository = productRepository;
            _paymentStatusService = paymentStatusService;
            _orderDetailRepository = orderDetailRepository;
            _paymentRepository = paymentRepository;
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
        //ReadyToShip->Shipped
        public Result<bool> MarkShipped (int orderId) 
        {
            var order =_orderRepository.GetEntityById(orderId);
            if(order == null) 
            {  return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");  }
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
                    "找不到已出貨狀態設定。"
                );
            }
            var errorMessage = order.MarkShipped
                (targetStatusResult.Data.OrderStatusId,
                currentStatusResult.Data.OrderStatusCode
                );
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, errorMessage);
            }
            _orderRepository.SaveChanges();
            return Result<bool>.Success(true, "訂單狀態已更新為已出貨。");
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
        //Arrived->PickedUp
        public Result<bool> MarkPickedUp(int orderId)
        {
            var order = _orderRepository.GetEntityById(orderId);
            if (order == null)
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單");
            }
            //查Order的OrderStatusId目前訂單狀態
            var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
            if (!currentStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError,
            "查詢目前訂單狀態失敗。");
            }
            //查要改的StatusCode「已取貨」狀態
            var targetStatusResult = _orderStatusService.GetByCode("PickedUp");
            if (!targetStatusResult.IsSuccess)
            {
                return Result<bool>.Fail(ErrorCodes.SystemError,
                    "找不到已到店狀態設定。");
            }
            //改orderstatusId
            var result = order.MarkPickedUp
                (
                 targetStatusResult.Data.OrderStatusId,
                currentStatusResult.Data.OrderStatusCode

                );
            if (!string.IsNullOrEmpty(result))
            {
                return Result<bool>.Fail(ErrorCodes.Conflict, result);
            }
            _orderRepository.SaveChanges();
            return Result<bool>.Success(true, "訂單狀態已更新為已取貨。");
        }
        //取消訂單
        public Result<bool> CancelOrder (int orderId,string cancelReson) 
        {
            //取消原因必填
            if (string.IsNullOrWhiteSpace(cancelReson))
            {
                return Result<bool>.Fail(ErrorCodes.Validation, "取消原因必填");
            }
            //找orderId
            var order = _orderRepository.GetEntityById(orderId);
            if (order == null)
            { return Result<bool>.Fail(ErrorCodes.Validation, "找不到訂單"); }
            //找order.statusId的code、name
            var currentStatusResult = _orderStatusService.GetById(order.OrderStatusId);
if(!currentStatusResult.IsSuccess) 
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "找不到物流狀態");
            }
            //找Cancelled對應的id、name
            var targetStatusResult = _orderStatusService.GetByCode("Cancelled");
            if(!targetStatusResult.IsSuccess) 
            {
                return Result<bool>.Fail(ErrorCodes.SystemError, "找不到取消訂單");
            }
            //把code、id丟給ORDER.CS判斷，不是就回錯誤訊息
            var errorMessage = order.CancelOrder(
                targetStatusResult.Data.OrderStatusId, currentStatusResult.Data.OrderStatusCode);
            if (!string.IsNullOrEmpty(errorMessage)) 
            {
                return Result<bool>.Fail(ErrorCodes.Conflict,errorMessage);
            }
            order.CancelReason = cancelReson;
            _orderRepository.SaveChanges();
            return Result<bool>.Success(true, "訂單已取消。");
            
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
            var tran=_db.Database.BeginTransaction();
            try
            {
                //下單流程
                var now = DateTime.Now;
                const int shippingFee = 60;
                // 查商品
                var product =_productRepository.GetEntityById(dto.ProductId);
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
                // 3. 預留庫存
                product.StockReserved += dto.Quantity;
                // ProductVersion+1
                product.ProductVersion += 1;
                // 4. 計算金額
                var unitPrice = product.Price;
                var subTotal = unitPrice * dto.Quantity;
                var orderTotal = subTotal + shippingFee;
                // 5. 取得初始訂單狀態 Processing
                var orderStatusResult = _orderStatusService.GetByCode("Processing");
                if (!orderStatusResult.IsSuccess)
                {
                    return Result<int>.Fail(ErrorCodes.SystemError, "找不到訂單狀態");
                }
                // 6. 取得初始付款狀態 Pending
                var paymentStatusResult = _paymentStatusService.GetByCode("Pending");
                if (!paymentStatusResult.IsSuccess) 
                {
                    return Result<int>.Fail(ErrorCodes.SystemError, "找不到付款狀態");
                }
                // 7. 建立 Order
                var order = new Order
                {
                    OrderNo = CreateOrderNo(),
                    BuyerUserId = dto.BuyerUserId,
                    SellerUserId = dto.SellerUserId,
                    OrderSource = 1,
                    PaymentMethod = dto.PaymentMethod,
                    CreatedAt = now,
                    ShippingFee = shippingFee,
                    OrderTotal = orderTotal,
                    PaymentDueAt = now.AddMinutes(15)
                };
                // 狀態初始化走 Order 封裝
                order.InitProcessing(orderStatusResult.Data.OrderStatusId);
                // 產生OrderId 
                _orderRepository.Add(order);
                _orderRepository.SaveChanges();
                // 8. 建立 OrderDetail
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

                // 9. 建立 Payment
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    TradeNo= CreateTradeNo(),
                    Amount=orderTotal,
                    PaidAt= null,
                    RawCallBack = null,
                    CreatedAt = now,
                    PaymentProvider= "測試"
                };
                payment.InitPending(paymentStatusResult.Data.PaymentStatusId);
                _paymentRepository.Add(payment);
                // 10. 存 OrderDetail + Payment + Product.StockReserved
                _orderRepository.SaveChanges();
                tran.Commit();
                return Result<int>.Success(order.OrderId);
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return Result<int>.Fail(ErrorCodes.SystemError, "下單失敗，請稍後再試");
            }
            finally
            {
                tran.Dispose();
            }
        }
        //訂單編號
        public string CreateOrderNo()
        {
            return "ORD"
                + DateTime.Now.ToString("yyyyMMddHHmmss")
                + new Random().Next(1000, 9999);
        }
        //交易編號
        public string CreateTradeNo()
        {
            return "TRADE"
                + DateTime.Now.ToString("yyyyMMddHHmmss")
                + new Random().Next(1000, 9999);
        }
    }
}