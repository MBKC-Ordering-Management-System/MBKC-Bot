using MBKC.Repository.GrabFoods.Models;
using MBKC.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.DTOs.Orders
{
    public class GetOrdersFromGrabFood
    {
        public List<FailedGrabFoodOrderDetail> FailedOrders { get; set; }
        public List<Order> Orders { get; set; }
    }
}
