using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodItemInfo
    {
        public int Count { get; set; }
        public List<GrabFoodItem> Items { get; set; }
    }
}
