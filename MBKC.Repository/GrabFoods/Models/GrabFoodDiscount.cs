using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodDiscount
    {
        public string DiscountName { get; set; }
        public string DiscountFunding { get; set; }
        public string ItemDiscountPriceDisplay { get; set; }
        public bool IsNewPromotion { get; set; }
        public string DiscountType { get; set; }
    }
}
