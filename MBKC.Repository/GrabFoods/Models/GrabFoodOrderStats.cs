using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodOrderStats
    {
        public int? NumberInReady { get; set; }
        public int? NumberInPrepare { get; set; }
        public int? NumberInUpComing { get; set; }
    }
}
