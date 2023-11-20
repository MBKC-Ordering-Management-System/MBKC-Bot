using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodUserProfile
    {
        public string? Role { get; set; }
        public string? Service_Id { get; set; }
        public string? Grab_Id { get; set; }
        public string? Grab_Food_Entity_Id { get; set; }
        public string? Parent_Entity_Id { get; set; }
    }
}
