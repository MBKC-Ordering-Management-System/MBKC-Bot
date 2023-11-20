using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.Models
{
    public class Configuration
    {
        public int Id { get; set; }
        public TimeSpan ScrawlingOrderStartTime { get; set; }
        public TimeSpan ScrawlingOrderEndTime { get; set; }
    }
}
