using MBKC.Repository.Enums;
using MBKC.Repository.GrabFoods.Models;
using MBKC.Repository.Infrastructures;
using MBKC.Repository.Models;
using MBKC.Service.DTOs.Orders;
using MBKC.Service.Services.Implementations;
using MBKC.Service.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;

namespace MBKC.WokerService
{
    public class Program
    {
        private static IStoreService _storeService;
        private static IAuthenticationService _authenticationService;
        private static IOrderService _orderService;
        private static IEmailService _emailService;
        private static IUserDeviceService _userDeviceService;
        private static List<string> _failedOrderIds;
        public static async void Main(string[] args)
        {
            IUnitOfWork unitOfWork = new UnitOfWork();
            _storeService = new StoreService(unitOfWork);
            _authenticationService = new AuthenticationService(unitOfWork);
            _orderService = new OrderService(unitOfWork);
            _emailService = new EmailService(unitOfWork);
            _userDeviceService = new UserDeviceService(unitOfWork);
            _failedOrderIds = new List<string>();

            List<Store> stores = _storeService.GetStoresAsync().Result;
            if (stores is not null)
            {
                foreach (var store in stores)
                {
                    if (store.StoreId == 1)
                    {
                        List<FailedOrder> failedOrders = new List<FailedOrder>();
                        if (store.StorePartners is not null && store.StorePartners.Count() > 0)
                        {
                            foreach (var storePartner in store.StorePartners)
                            {
                                if (storePartner.PartnerId == (int)PartnerEnum.Type.GRABFOOD)
                                {
                                    //GrabFoodAuthenticationResponse grabFoodAuthenticationResponse = await _authenticationService.LoginAsync(storePartner.UserName, storePartner.Password);
                                    //if (grabFoodAuthenticationResponse is not null && grabFoodAuthenticationResponse.Data.Success)
                                    //{
                                    //GetOrdersFromGrabFood ordersFromGrabFood = await _orderService.GetOrdersFromGrabFoodAsync(grabFoodAuthenticationResponse, store, storePartner);
                                    using StreamReader reader = new("E:\\FPTUniversity\\SU2023\\PRN231\\Projects\\MBKC-Bot\\MBKCBot\\MBKCBot.Test\\orderData.json");
                                    var json = reader.ReadToEnd();
                                    List<GrabFoodOrderDetailResponse> list = new List<GrabFoodOrderDetailResponse>();

                                    GrabFoodOrderDetailResponse grabFoodOrderDetailResponse = JsonConvert.DeserializeObject<GrabFoodOrderDetailResponse>(json);
                                    grabFoodOrderDetailResponse.Order.Status = "Preparing";
                                    list.Add(grabFoodOrderDetailResponse);
                                    GetOrdersFromGrabFood ordersFromGrabFood = _orderService.GetOrdersFromGrabFoodAsync(list, store, storePartner).Result;

                                    if (ordersFromGrabFood.FailedOrders is not null && ordersFromGrabFood.FailedOrders.Count > 0)
                                    {
                                        foreach (var failedOrder in ordersFromGrabFood.FailedOrders)
                                        {
                                            if (_failedOrderIds.Contains(failedOrder.OrderId) == false)
                                            {
                                                _failedOrderIds.Add(failedOrder.OrderId);
                                                failedOrders.Add(new FailedOrder()
                                                {
                                                    OrderId = failedOrder.OrderId,
                                                    Reason = failedOrder.Reason,
                                                    PartnerName = storePartner.Partner.Name
                                                });
                                            }
                                        }
                                    }
                                    if (ordersFromGrabFood.Orders is not null && ordersFromGrabFood.Orders.Count > 0)
                                    {
                                        foreach (var order in ordersFromGrabFood.Orders)
                                        {
                                            Tuple<Order, bool> existedOrderTuple = _orderService.GetOrderAsync(order.OrderPartnerId).Result;
                                            Order existedOrder = existedOrderTuple.Item1;
                                            bool isSuccessed = existedOrderTuple.Item2;
                                            Log.Information("Existed Order: {Order}", existedOrder);
                                            if (isSuccessed)
                                            {
                                                if (existedOrder is not null && string.IsNullOrWhiteSpace(existedOrder.OrderPartnerId) == false && existedOrder.PartnerOrderStatus.ToLower().Equals("upcoming"))
                                                {
                                                    //update
                                                    Log.Information("Update existed Order. => Data: {data}");
                                                    Order updatedOrder = _orderService.UpdateOrderAsync(order).Result;
                                                    string title = $"Đã tới thời gian cho đơn hàng đặt trước: {order.DisplayId}";
                                                    string body = $"Vui lòng bắt tay chuẩn bị đơn hàng ngay.";
                                                    _userDeviceService.PushNotificationAsync(title, body, updatedOrder.Id, store.UserDevices);
                                                }
                                                else if (existedOrder is null)
                                                {
                                                    //create new
                                                    Log.Information("Create new Order. => Data: {data}", order);
                                                    Order createdOrder = _orderService.CreateOrderAsync(order).Result;
                                                    string title = $"Có đơn hàng mới: {order.DisplayId}";
                                                    string body = $"Vui lòng bắt tay chuẩn bị đơn hàng ngay.";
                                                    _userDeviceService.PushNotificationAsync(title, body, createdOrder.Id, store.UserDevices);
                                                }
                                            }
                                        }
                                    }
                                    Log.Information("Worker running at: {time} - failed orders: {Data}", DateTimeOffset.Now, JsonConvert.SerializeObject(failedOrders));
                                    Log.Information("Worker running at: {time} - orders: {Data}", DateTimeOffset.Now, JsonConvert.SerializeObject(ordersFromGrabFood.Orders));
                                    //}
                                }
                            }
                            if (failedOrders.Count > 0)
                            {
                                //send email
                                Log.Information("Store Fail: {store}", store);
                                _emailService.SendEmailForFailedOrderAsync(failedOrders, store.StoreManagerEmail);
                            }
                        }
                    }
                }
            }
        }
    }
}