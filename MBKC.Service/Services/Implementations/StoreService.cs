using MBKC.Repository.Infrastructures;
using MBKC.Repository.Models;
using MBKC.Service.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Implementations
{
    public class StoreService: IStoreService
    {
        private UnitOfWork _unitOfWork;
        public StoreService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<List<Store>> GetStoresAsync()
        {
            try
            {
                Log.Information("Processing in StoreService to get stores.");
                List<Store> stores = await this._unitOfWork.StoreRepository.GetStoresAsync();
                Log.Information("Getting stores successfully in StoreService. => Data: {Data}", JsonConvert.SerializeObject(stores));
                return stores;
            } catch(Exception ex)
            {
                Log.Error("Error in StoreService => Exception: {Message}", ex.Message);
                return null;
            }
        }
    }
}
