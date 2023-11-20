using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodOrderFare
    {
        public string TotalDisplay { get; set; }
        public string SubTotalDisplay { get; set; }
        public string TaxDisplay { get; set; }
        public string PromotionDisplay { get; set; }
        public string DeliveryFeeDisplay { get; set; }
        public string PassengerTotalDisplay { get; set; }
        public string TotalDiscountAmountDisplay { get; set; }
        public string ReducedPriceDisplay { get; set; }
        public string RevampedSubtotalDisplay { get; set; }
    }
}
