using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodAuthentication
    {
        public bool Success { get; set; }
        public int? Code { get; set; }
        public GrabFoodToken Data { get; set; }
    }
}
