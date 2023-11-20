using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodEater
    {
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string Comment { get; set; }
        public GrabFoodEaterAddress Address { get; set; }

    }
}
