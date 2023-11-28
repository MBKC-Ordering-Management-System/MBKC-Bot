using MBKC.Repository.GrabFoods.Models;
using MBKC.Repository.MBKCs.Models;
using MBKC.Repository.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.Repositories
{
    public class OrderRepository
    {
        private PrivateAPIs _privateAPIs;
        private HttpClient _httpClient;
        public OrderRepository(PrivateAPIs privateAPIs)
        {
            this._privateAPIs = privateAPIs;
            this._httpClient = new HttpClient();
        }


        public async Task<Order> GetOrderAsync(string partnerOrderId)
        {
            try
            {
                Log.Information("Processing in OrderRepository to get order.");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._privateAPIs.OrderAPI}/{partnerOrderId}");
                Log.Information("Processing in OrderRepository to Call API from MBKC Private API");
                HttpResponseMessage response = await this._httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    Order order = JsonConvert.DeserializeObject<Order>(responseText);
                    Log.Information("Getting order successfully in OrderRepository => Data: {Data}", responseText);
                    return order;
                }
                else if((int)response.StatusCode == StatusCodes.Status404NotFound)
                {
                    return null;
                }
                else
                {
                    throw new Exception($"Accessing to MBKC Private API Failed in OrderRepository Failed. {response.Content.ReadAsStringAsync()}");
                }
            } catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            try
            {
                Log.Information("Processing in OrderRepository to create order.");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this._privateAPIs.OrderAPI);
                Log.Information("Processing in OrderRepository to Call API from MBKC Private API");
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await this._httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    Order createdOrder = JsonConvert.DeserializeObject<Order>(responseText);
                    Log.Information("Created order successfully in OrderRepository.");
                    return createdOrder;
                }
                else
                {
                    throw new Exception($"Accessing to MBKC Private API Failed in OrderRepository Failed. {response.Content.ReadAsStringAsync().Result}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public async Task<Order> UpdateOrderAsync(Order order)
        {
            try
            {
                Log.Information("Processing in OrderRepository to update order.");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"{this._privateAPIs.OrderAPI}/{order.OrderPartnerId}");
                Log.Information("Processing in OrderRepository to Call API from MBKC Private API");
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                UpdateOrderRequest updateOrderRequest = new UpdateOrderRequest() { Status = order.PartnerOrderStatus };
                request.Content = new StringContent(JsonConvert.SerializeObject(updateOrderRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await this._httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    Order updatedOrder = JsonConvert.DeserializeObject<Order>(responseText);
                    Log.Information("Updated order successfully in OrderRepository.");
                    return updatedOrder;
                }
                else
                {
                    throw new Exception($"Accessing to MBKC Private API Failed in OrderRepository Failed. {response.Content.ReadAsStringAsync().Result}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
