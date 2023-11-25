using MBKC.Repository.GrabFoods.Models;
using MBKC.Repository.Infrastructures;
using MBKC.Service.Services.Interfaces;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Implementations
{
    public class AuthenticationService: IAuthenticationService
    {
        private UnitOfWork _unitOfWork;
        public AuthenticationService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<GrabFoodAuthenticationResponse> LoginGrabFoodAsync(string username, string password)
        {
            try
            {
                GrabFoodAccount grabFoodAccount = new GrabFoodAccount()
                {
                    Username = username,
                    Password = password
                };
                Log.Information("Processing in AuthenticationService to get configurations.");
                GrabFoodAuthenticationResponse grabFoodAuthenticationResponse = await this._unitOfWork.GrabFoodRepository.LoginAsync(grabFoodAccount);
                Log.Information("Logging into GrabFood Partner successfully in AuthenticationService => Data: {Data}", JsonConvert.SerializeObject(grabFoodAuthenticationResponse));
                return grabFoodAuthenticationResponse;
            } catch(Exception ex)
            {
                Log.Error("Error in AuthenticationService => Exception: {Message}", ex.Message);
                return null;
            }
        }
    }
}
