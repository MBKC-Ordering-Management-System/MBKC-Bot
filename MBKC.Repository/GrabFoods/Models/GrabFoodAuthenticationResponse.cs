using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodAuthenticationResponse
    {
        public GrabFoodAuthentication Data { get; set; }
        public GrabFoodError Error { get; set; }
    }
}
