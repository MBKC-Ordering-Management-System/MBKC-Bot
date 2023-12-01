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
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Start Getting Stores.");
                Console.ResetColor();
                List<Store> stores = await this._unitOfWork.StoreRepository.GetStoresAsync();
                Log.Information("Getting stores successfully in StoreService. => Data: {Data}", JsonConvert.SerializeObject(stores));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Get Stores Successfully.");
                Console.ResetColor();
                return stores;
            } catch(Exception ex)
            {
                Log.Error("Error in StoreService => Exception: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Get Stores Failed.");
                Console.ResetColor();
                return null;
            }
        }
        
        public async Task<List<Store>> GetStoresAsync_Tool()
        {
            try
            {
                Log.Information("Processing in StoreService to get stores.");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Start Getting Stores.");
                Console.ResetColor();
                List<Store> stores = await this._unitOfWork.StoreRepository.GetStoresAsync();
                Log.Information("Getting stores successfully in StoreService. => Data: {Data}", JsonConvert.SerializeObject(stores));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Get Stores Successfully.");
                Console.ResetColor();
                return stores;
            } catch(Exception ex)
            {
                Log.Error("Error in StoreService => Exception: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Get Stores Failed.");
                Console.ResetColor();
                throw new Exception(ex.Message);
            }
        }
    }
}
