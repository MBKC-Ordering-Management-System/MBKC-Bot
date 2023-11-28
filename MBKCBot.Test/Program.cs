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
        public static void Main(string[] args)
        {
            IUnitOfWork unitOfWork = new UnitOfWork();
            _storeService = new StoreService(unitOfWork);
            _authenticationService = new AuthenticationService(unitOfWork);
            _orderService = new OrderService(unitOfWork);
            _emailService = new EmailService(unitOfWork);
            _userDeviceService = new UserDeviceService(unitOfWork);
            _failedOrderIds = new List<string>();

            bool isSucceeded = false;
            int storeId = 0;
            do
            {
                try
                {
                    Console.WriteLine("Type a Store Id: ");
                    storeId = Convert.ToInt32(Console.ReadLine());
                    isSucceeded = true;
                    if (storeId <= 0)
                    {
                        Console.WriteLine("Please type store id that is greater than 0.");
                        isSucceeded = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Please type valid integer number.");
                    isSucceeded = false;
                }
            } while (isSucceeded == false);
            isSucceeded = false;
            string statusOrder = "";
            do
            {
                try
                {
                    Console.WriteLine("Choose a Order Status: \n" +
                "1: Preparing\n" +
                "2: Upcoming");
                    int numberOfStatus = Convert.ToInt32(Console.ReadLine());
                    switch (numberOfStatus)
                    {
                        case 1:
                            {
                                statusOrder = "preparing";
                                isSucceeded = true;
                                break;
                            }
                        case 2:
                            {
                                statusOrder = "upcoming";
                                isSucceeded = true;
                                break;
                            }
                    }
                    if (isSucceeded == false)
                    {
                        Console.WriteLine("Please choose 1 or 2 option!");
                        isSucceeded = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Please type valid integer number.");
                    isSucceeded = false;
                }
            } while (isSucceeded == false);

            List<Store> stores = _storeService.GetStoresAsync().Result;
            if (stores is not null)
            {
                foreach (var store in stores)
                {
                    if (store.StoreId == storeId)
                    {
                        List<FailedOrder> failedOrders = new List<FailedOrder>();
                        if (store.StorePartners is not null && store.StorePartners.Count() > 0)
                        {
                            foreach (var storePartner in store.StorePartners)
                            {
                                if (storePartner.PartnerId == (int)PartnerEnum.Type.GRABFOOD)
                                {
                                    FileInfo fileInfo = new FileInfo("orderData.json");
                                    string fullPath = fileInfo.FullName;
                                    using StreamReader reader = new(fullPath);
                                    var json = reader.ReadToEnd();
                                    List<GrabFoodOrderDetailResponse> list = new List<GrabFoodOrderDetailResponse>();

                                    GrabFoodOrderDetailResponse grabFoodOrderDetailResponse = JsonConvert.DeserializeObject<GrabFoodOrderDetailResponse>(json);
                                    grabFoodOrderDetailResponse.Order.Status = statusOrder;
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
                                            if (isSuccessed)
                                            {
                                                if (existedOrder is not null && string.IsNullOrWhiteSpace(existedOrder.OrderPartnerId) == false && existedOrder.PartnerOrderStatus.ToLower().Equals("upcoming"))
                                                {
                                                    //update
                                                    Order updatedOrder = _orderService.UpdateOrderAsync(order).Result;
                                                    if (updatedOrder is not null)
                                                    {
                                                        string title = $"Đã tới thời gian cho đơn hàng đặt trước: {order.DisplayId}";
                                                        string body = $"Vui lòng bắt tay chuẩn bị đơn hàng ngay.";
                                                        _userDeviceService.PushNotificationAsync(title, body, updatedOrder.Id, store.UserDevices);
                                                    }
                                                }
                                                else if (existedOrder is null)
                                                {
                                                    //create new
                                                    Order createdOrder = _orderService.CreateOrderAsync(order).Result;
                                                    if (createdOrder is not null)
                                                    {
                                                        string title = $"Có đơn hàng mới: {order.DisplayId}";
                                                        string body = $"Vui lòng bắt tay chuẩn bị đơn hàng ngay.";
                                                        if (createdOrder.PartnerOrderStatus.ToLower().Equals("upcoming"))
                                                        {
                                                            title = $"Có đơn hàng đặt trước mới: {order.DisplayId}";
                                                            body = $"Vui lòng chờ đến khi thời gian chuẩn bị bắt đầu.";
                                                        }
                                                        _userDeviceService.PushNotificationAsync(title, body, createdOrder.Id, store.UserDevices);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (failedOrders.Count > 0)
                            {
                                //send email
                                Log.Information("Start Send Email about Failed Orders.");
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine("Start Send Email about Failed Orders.");
                                Console.ResetColor();
                                _emailService.SendEmailForFailedOrderAsync(failedOrders, store.StoreManagerEmail);
                            }
                        }
                    }
                }
            }
        }
    }
}