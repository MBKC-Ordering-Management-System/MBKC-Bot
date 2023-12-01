using MBKC.Repository.GrabFoods.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKCBot.Tool.ViewModels
{
    public class OrderViewModel
    {
        public List<GrabFoodOrderDetailResponse> Orders { get; set; }
    }
}
