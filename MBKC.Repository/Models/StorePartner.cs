using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.Models
{
    public class StorePartner
    {
        public int StoreId { get; set; }
        public int PartnerId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public float Commission { get; set; }
        public Partner Partner { get; set; }
        public List<PartnerProduct> PartnerProducts { get; set; }
    }
}
