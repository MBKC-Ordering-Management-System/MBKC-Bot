using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodOrderDetail
    {
        public string OrderId { get; set; }
        public string DisplayID { get; set; }
        public GrabFoodDriver Driver { get; set; }
        public GrabFoodEater Eater { get; set; }
        public GrabFoodItemInfo ItemInfo { get; set; }
        public GrabFoodOrderFare Fare { get; set; }
        public string PaymentMethod { get; set; }
        public int Cutlery { get; set; }
        public string Status { get; set; }
    }
}
