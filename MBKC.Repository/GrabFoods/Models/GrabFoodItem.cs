using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodItem
    {
        public string ItemId { get; set; }
        public string ItemCode { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Comment { get; set; }
        public List<GrabFoodModifierGroup> ModifierGroups { get; set; }
        public List<GrabFoodDiscount> DiscountInfo { get; set; }
    }
}
