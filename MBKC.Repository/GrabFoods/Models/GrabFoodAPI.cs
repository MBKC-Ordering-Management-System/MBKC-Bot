using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodAPI
    {
        public string AuthenticationURI { get; set; }
        public string OrderURI { get; set; }
        public string OrderDetailURI { get; set; }
        public string RequestSource { get; set; }
    }
}
