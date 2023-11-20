using MBKC.Repository.MBKCs.Models;
using MBKC.Repository.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.Repositories
{
    public class StoreRepository
    {
        private PrivateAPIs _privateAPIs;
        private HttpClient _httpClient;
        public StoreRepository(PrivateAPIs privateAPIs)
        {
            this._privateAPIs = privateAPIs;
            this._httpClient = new HttpClient();
        }

        public async Task<List<Store>> GetStoresAsync()
        {
            try
            {
                Log.Information("Processing in StoreRepository to get stores.");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, this._privateAPIs.StoreAPI);
                Log.Information("Processing in StoreRepository to Call API from MBKC Private API");
                HttpResponseMessage response = await this._httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    List<Store> stores = JsonConvert.DeserializeObject<List<Store>>(responseText);
                    Log.Information("Getting configurations successfully in StoreRepository => Data: {Data}", responseText);
                    return stores;
                }
                else
                {
                    throw new Exception("Accessing to MBKC Private API Failed in StoreRepository Failed.");
                }
            } catch(Exception ex)
            {
                Log.Error("Error in StoreRepository => Exception: {Message}", ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
