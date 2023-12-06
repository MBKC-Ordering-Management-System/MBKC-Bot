using MBKC.Repository.Enums;
using MBKC.Repository.GrabFoods.Models;
using MBKC.Repository.Infrastructures;
using MBKC.Repository.Models;
using MBKC.Service.DTOs.Orders;
using MBKC.Service.Services.Implementations;
using MBKC.Service.Services.Interfaces;
using MBKCBot.Tool.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MBKCBot.Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IStoreService _storeService;
        private IAuthenticationService _authenticationService;
        private IOrderService _orderService;
        private IEmailService _emailService;
        private IUserDeviceService _userDeviceService;
        private List<string> _failedOrderIds;
        private string _orderStatus;
        private int _storeId;
        private string _fileName;
        public MainWindow()
        {
            InitializeComponent();
            IUnitOfWork unitOfWork = new UnitOfWork();
            this._storeService = new StoreService(unitOfWork);
            this._authenticationService = new AuthenticationService(unitOfWork);
            this._orderService = new OrderService(unitOfWork);
            this._emailService = new EmailService(unitOfWork);
            this._userDeviceService = new UserDeviceService(unitOfWork);
            this._failedOrderIds = new List<string>();
        }

        private async void Support_Tool_Loaded(object sender, RoutedEventArgs e)
        {
            List<Store> stores = await this._storeService.GetStoresAsync();
            List<StoreViewModel> storeViewModels = new List<StoreViewModel>();
            foreach (var store in stores)
            {
                string[] addressParts = store.Address.Split(",");
                List<string> newAddressParts = addressParts.SkipLast(3).ToList();
                string newAddress = "";
                foreach (var newAddressPart in newAddressParts)
                {
                    if (newAddressParts.Last().Equals(newAddressPart))
                    {
                        newAddress += newAddressPart + ".";
                    }
                    else
                    {
                        newAddress += newAddressPart + ",";
                    }
                }
                storeViewModels.Add(new StoreViewModel
                {
                    Id = store.StoreId,
                    Name = store.Name,
                    Address = newAddress,
                    Logo = new BitmapImage(new Uri(store.Logo, UriKind.Absolute))
                });
            }
            this.cbStores.ItemsSource = storeViewModels;
        }

        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect = false };
            bool? response = openFileDialog.ShowDialog();
            if(response == true)
            {
                string fileName = openFileDialog.FileName;
                this.fileName.Text = System.IO.Path.GetFileName(fileName);
                this._fileName = fileName;
            }
        }

        private async void btnPushOrder_Click(object sender, RoutedEventArgs e)
        {
            StoreViewModel selectedStore = this.cbStores.SelectedItem as StoreViewModel;
            if(selectedStore is null)
            {
                MessageBox.Show("Please select store!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            this._storeId = selectedStore.Id;
            bool? rbtnPreparing = this.rbtnPreparing.IsChecked;
            bool? rbtnUpcoming = this.rbtnUpcoming.IsChecked;
            if(rbtnPreparing == false && rbtnUpcoming == false)
            {
                MessageBox.Show("Please choose order status!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(rbtnPreparing == true)
            {
                this._orderStatus = "Preparing";
            }
            if(rbtnUpcoming == true)
            {
                this._orderStatus = "Upcoming";
            }
            try
            {
                if (string.IsNullOrWhiteSpace(this._fileName) == false && this._fileName.EndsWith(".json"))
                {
                    using StreamReader reader = new(this._fileName);
                    var json = reader.ReadToEnd();
                    OrderViewModel orders = JsonConvert.DeserializeObject<OrderViewModel>(json);
                    BrushConverter bc = new BrushConverter();
                    try
                    {
                        this.btnPushOrder.IsEnabled = false;
                        this.btnPushOrder.BorderBrush = Brushes.Gray;
                        this.btnPushOrder.Foreground = Brushes.Gray;
                        List<Store> stores = await this._storeService.GetStoresAsync_Tool();
                        if (stores is not null)
                        {
                            foreach (var store in stores)
                            {
                                if (store.StoreId == this._storeId)
                                {
                                    List<FailedOrder> failedOrders = new List<FailedOrder>();
                                    if (store.StorePartners is not null && store.StorePartners.Count() > 0)
                                    {
                                        foreach (var storePartner in store.StorePartners)
                                        {
                                            if (storePartner.PartnerId == (int)PartnerEnum.Type.GRABFOOD)
                                            {
                                                foreach (var order in orders.Orders)
                                                {
                                                    order.Order.Status = this._orderStatus;
                                                }
                                                GetOrdersFromGrabFood ordersFromGrabFood = await this._orderService.GetOrdersFromGrabFoodAsync_Tool(orders.Orders, store, storePartner);

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
                                                        Tuple<Order, bool> existedOrderTuple = await this._orderService.GetOrderAsync_Tool(order.OrderPartnerId);
                                                        Order existedOrder = existedOrderTuple.Item1;
                                                        bool isSuccessed = existedOrderTuple.Item2;
                                                        if (isSuccessed)
                                                        {
                                                            if (existedOrder is not null && string.IsNullOrWhiteSpace(existedOrder.OrderPartnerId) == false && existedOrder.PartnerOrderStatus.ToLower().Equals("upcoming"))
                                                            {
                                                                //update
                                                                Order updatedOrder = await this._orderService.UpdateOrderAsync_Tool(order);
                                                                if (updatedOrder is not null)
                                                                {
                                                                    string title = $"Đã tới thời gian cho đơn hàng đặt trước: {order.DisplayId}";
                                                                    string body = $"Vui lòng bắt tay chuẩn bị đơn hàng ngay.";
                                                                    await this._userDeviceService.PushNotificationAsync_Tool(title, body, updatedOrder.Id, store.UserDevices);
                                                                }
                                                            }
                                                            else if (existedOrder is null)
                                                            {
                                                                //create new
                                                                Order createdOrder = await this._orderService.CreateOrderAsync_Tool(order);
                                                                if (createdOrder is not null)
                                                                {
                                                                    string title = $"Có đơn hàng mới: {order.DisplayId}";
                                                                    string body = $"Vui lòng bắt tay chuẩn bị đơn hàng ngay.";
                                                                    if (createdOrder.PartnerOrderStatus.ToLower().Equals("upcoming"))
                                                                    {
                                                                        title = $"Có đơn hàng đặt trước mới: {order.DisplayId}";
                                                                        body = $"Vui lòng chờ đến khi thời gian chuẩn bị bắt đầu.";
                                                                    }
                                                                    await this._userDeviceService.PushNotificationAsync_Tool(title, body, createdOrder.Id, store.UserDevices);
                                                                }
                                                            } else
                                                            {
                                                                MessageBox.Show("Push Order Failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                                this.btnPushOrder.IsEnabled = true;
                                                                this.btnPushOrder.BorderBrush = Brushes.Black;
                                                                this.btnPushOrder.Foreground = (Brush)bc.ConvertFrom("#FFEC407A");
                                                                return;
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
                                            await this._emailService.SendEmailForFailedOrderAsync(failedOrders, store.StoreManagerEmail);
                                        }
                                    }
                                }
                            }
                        }
                    } catch(Exception ex)
                    {
                        MessageBox.Show("Push Order Failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.btnPushOrder.IsEnabled = true;
                        this.btnPushOrder.BorderBrush = Brushes.Black;
                        this.btnPushOrder.Foreground = (Brush)bc.ConvertFrom("#FFEC407A");
                        return;
                    }
                    MessageBox.Show("Push Order Successfully!", "Success", MessageBoxButton.OK);
                    this.btnPushOrder.IsEnabled = true;
                    this.btnPushOrder.BorderBrush = Brushes.Black;
                    this.btnPushOrder.Foreground = (Brush)bc.ConvertFrom("#FFEC407A");
                }

                if (string.IsNullOrWhiteSpace(this._fileName))
                {
                    MessageBox.Show("Please choose file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if(string.IsNullOrWhiteSpace(this._fileName) == false && this._fileName.EndsWith(".json") == false)
                {
                    MessageBox.Show("File is required a JSON file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            } catch(Exception ex)
            {
                MessageBox.Show("File does not match with GrabFood Format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
