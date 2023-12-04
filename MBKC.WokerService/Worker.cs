using MBKC.Repository.Enums;
using MBKC.Repository.GrabFoods.Models;
using MBKC.Repository.Models;
using MBKC.Service.DTOs.Orders;
using MBKC.Service.Services.Interfaces;
using MBKC.Service.Utils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Data;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;

namespace MBKC.WokerService
{
    public class Worker : BackgroundService
    {
        private IConfigurationService _configurationService;
        private IStoreService _storeService;
        private IAuthenticationService _authenticationService;
        private IOrderService _orderService;
        private IEmailService _emailService;
        private IUserDeviceService _userDeviceService;
        private IRabbitMQService _rabbitMQService;
        private List<Configuration> _configurations;
        private List<string> _failedOrderIds;
        public Worker(IConfigurationService configurationService, IStoreService storeService,
            IAuthenticationService authenticationService, IOrderService orderService, IEmailService emailService,
            IRabbitMQService rabbitMQService, IUserDeviceService userDeviceService)
        {
            this._configurationService = configurationService;
            this._storeService = storeService;
            this._authenticationService = authenticationService;
            this._orderService = orderService;
            this._emailService = emailService;
            this._rabbitMQService = rabbitMQService;
            this._userDeviceService = userDeviceService;
        }

        private static JObject GetWebjobState()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                                  .SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            string userName = configuration.GetSection("Azure:WebJob:Username").Value;
            string userPWD = configuration.GetSection("Azure:WebJob:Password").Value;
            string webjobUrl = configuration.GetSection("Azure:WebJob:WebHook").Value;
            HttpClient client = new HttpClient();
            string auth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ':' + userPWD));
            client.DefaultRequestHeaders.Add("authorization", auth);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var data = client.GetStringAsync(webjobUrl).Result;
            var result = JsonConvert.DeserializeObject(data) as JObject;
            return result;
        }

        public override async Task<Task> StartAsync(CancellationToken cancellationToken)
        {
            this._configurations = await this._configurationService.GetConfigurationsAsync();
            this._failedOrderIds = new List<string>();
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    JObject result = GetWebjobState();
                    Log.Information("WebJob in Azure: {WebJob}", result);
                    Configuration configuration = this._rabbitMQService.ReceiveMessage("Modified_Configurations");
                    if (configuration is not null)
                    {
                        this._configurations.First().ScrawlingOrderStartTime = configuration.ScrawlingOrderStartTime;
                        this._configurations.First().ScrawlingOrderEndTime = configuration.ScrawlingOrderEndTime;
                    }
                    if (this._configurations is not null)
                    {
                        if (DateTime.Now.TimeOfDay >= this._configurations.First().ScrawlingOrderStartTime && DateTime.Now.TimeOfDay <= this._configurations.First().ScrawlingOrderEndTime)
                        {
                            //get stores
                            List<Store> stores = await this._storeService.GetStoresAsync();
                            if (stores is not null)
                            {
                                foreach (var store in stores)
                                {
                                    List<FailedOrder> failedOrders = new List<FailedOrder>();
                                    if (store.StorePartners is not null && store.StorePartners.Count() > 0)
                                    {
                                        foreach (var storePartner in store.StorePartners)
                                        {
                                            if (storePartner.PartnerId == (int)PartnerEnum.Type.GRABFOOD)
                                            {
                                                GrabFoodAuthenticationResponse grabFoodAuthenticationResponse = await this._authenticationService.LoginGrabFoodAsync(storePartner.UserName, storePartner.Password);
                                                if (grabFoodAuthenticationResponse is not null && grabFoodAuthenticationResponse.Data.Success)
                                                {
                                                    GetOrdersFromGrabFood ordersFromGrabFood = await this._orderService.GetOrdersFromGrabFoodAsync(grabFoodAuthenticationResponse, store, storePartner);
                                                    if (ordersFromGrabFood.FailedOrders is not null && ordersFromGrabFood.FailedOrders.Count > 0)
                                                    {
                                                        foreach (var failedOrder in ordersFromGrabFood.FailedOrders)
                                                        {
                                                            if (this._failedOrderIds.Contains(failedOrder.OrderId) == false)
                                                            {
                                                                this._failedOrderIds.Add(failedOrder.OrderId);
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
                                                            Tuple<Order, bool> existedOrderTuple = await this._orderService.GetOrderAsync(order.OrderPartnerId);
                                                            Order existedOrder = existedOrderTuple.Item1;
                                                            bool isSuccessed = existedOrderTuple.Item2;
                                                            Log.Information("Existed Order: {Order}", existedOrder);
                                                            if (isSuccessed)
                                                            {
                                                                if (existedOrder is not null && string.IsNullOrWhiteSpace(existedOrder.OrderPartnerId) == false && existedOrder.PartnerOrderStatus.ToLower().Equals("upcoming"))
                                                                {
                                                                    //update
                                                                    Log.Information("Update existed Order. => Data: {data}");
                                                                    Order updatedOrder = await _orderService.UpdateOrderAsync(order);
                                                                    if (updatedOrder is not null)
                                                                    {
                                                                        string title = $"Đã tới thời gian cho đơn hàng đặt trước: {order.DisplayId}";
                                                                        string body = $"Vui lòng bắt tay chuẩn bị đơn hàng ngay.";
                                                                        await _userDeviceService.PushNotificationAsync(title, body, updatedOrder.Id, store.UserDevices);
                                                                    }
                                                                }
                                                                else if (existedOrder is null)
                                                                {
                                                                    //create new
                                                                    Log.Information("Create new Order. => Data: {data}", order);
                                                                    Order createdOrder = await _orderService.CreateOrderAsync(order);
                                                                    if (createdOrder is not null)
                                                                    {
                                                                        string title = $"Có đơn hàng mới: {order.DisplayId}";
                                                                        string body = $"Vui lòng bắt tay chuẩn bị đơn hàng ngay.";
                                                                        if (createdOrder.PartnerOrderStatus.ToLower().Equals("upcoming"))
                                                                        {
                                                                            title = $"Có đơn hàng đặt trước mới: {order.DisplayId}";
                                                                            body = $"Vui lòng chờ đến khi thời gian chuẩn bị bắt đầu.";
                                                                        }
                                                                        await _userDeviceService.PushNotificationAsync(title, body, createdOrder.Id, store.UserDevices);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    Log.Information("Worker running at: {time} - failed orders: {Data}", DateTimeOffset.Now, JsonConvert.SerializeObject(failedOrders));
                                                    Log.Information("Worker running at: {time} - orders: {Data}", DateTimeOffset.Now, JsonConvert.SerializeObject(ordersFromGrabFood.Orders));
                                                }
                                            }
                                        }
                                        if (failedOrders.Count > 0)
                                        {
                                            //send email
                                            Log.Information("Store Fail: {store}", store);
                                            await this._emailService.SendEmailForFailedOrderAsync(failedOrders, store.StoreManagerEmail);
                                        }
                                    }
                                }
                            }
                            await Task.Delay(60 * 1000, stoppingToken);
                        }

                        if (DateTime.Now.TimeOfDay > this._configurations.FirstOrDefault().ScrawlingOrderEndTime)
                        {
                            this._failedOrderIds.Clear();
                        }
                    }
                    else
                    {
                        await Task.Delay(0, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in Worker => Message: ", ex.Message);
            }
        }
    }
}