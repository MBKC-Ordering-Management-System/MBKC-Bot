using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.DTOs.Orders
{
    public class FailedOrder
    {
        public string OrderId { get; set; }
        public string Reason { get; set; }
        public string PartnerName { get; set; }
    }
}
