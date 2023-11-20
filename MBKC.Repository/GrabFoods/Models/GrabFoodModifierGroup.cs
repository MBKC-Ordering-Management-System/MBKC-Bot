using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.GrabFoods.Models
{
    public class GrabFoodModifierGroup
    {
        public string ModifierGroupId { get; set; }
        public string ModifierGroupName { get; set; }
        public List<GrabFoodModifier> Modifiers { get; set; }
    }
}
