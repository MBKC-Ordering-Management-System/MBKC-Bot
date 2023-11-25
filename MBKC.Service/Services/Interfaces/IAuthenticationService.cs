using MBKC.Repository.GrabFoods.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<GrabFoodAuthenticationResponse> LoginGrabFoodAsync(string username, string password);
    }
}
