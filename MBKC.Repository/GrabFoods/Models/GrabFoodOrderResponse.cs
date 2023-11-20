using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodOrderResponse
    {
        public GrabFoodOrderStats OrderStats { get; set; }
        public List<GrabFoodOrder>? Orders { get; set; }
    }
}
