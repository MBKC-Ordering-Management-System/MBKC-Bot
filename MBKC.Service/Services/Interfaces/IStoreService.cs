using MBKC.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Interfaces
{
    public interface IStoreService
    {
        public Task<List<Store>> GetStoresAsync();
        public Task<List<Store>> GetStoresAsync_Tool();
    }
}
