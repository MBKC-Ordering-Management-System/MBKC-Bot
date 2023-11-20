using MBKC.Repository.GrabFoods.Models;
using MBKC.Repository.Models;
using MBKC.Service.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Interfaces
{
    public interface IOrderService
    {
        public Task<GetOrdersFromGrabFood> GetOrdersFromGrabFoodAsync(GrabFoodAuthenticationResponse grabFoodAuthentication, Store store, StorePartner storePartner);
        public Task<Tuple<Order, bool>> GetOrderAsync(string partnerOrderId);
        public Task<Order> CreateOrderAsync(Order order);
        public Task<Order> UpdateOrderAsync(Order order);

        public Task<GetOrdersFromGrabFood> GetOrdersFromGrabFoodAsync(List<GrabFoodOrderDetailResponse> grabFoodOrderDetails, Store store, StorePartner storePartner);
    }
}
