using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodToken
    {
        public string? JWT { get; set; }
        public GrabFoodUserProfile User_Profile { get; set; }
    }
}
