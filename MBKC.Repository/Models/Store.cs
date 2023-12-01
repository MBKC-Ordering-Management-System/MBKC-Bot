using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.Models
{
    public class Store
    {
        public int StoreId { get; set; }
        public string StoreManagerEmail { get; set; }
        public string Logo { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<StorePartner> StorePartners { get; set; }
        public List<UserDevice> UserDevices { get; set; }
    }
}
