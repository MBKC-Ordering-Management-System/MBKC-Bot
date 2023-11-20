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
    public class UserDeviceRepository
    {
        private PrivateAPIs _privateAPIs;
        private HttpClient _httpClient;
        public UserDeviceRepository(PrivateAPIs privateAPIs)
        {
            this._privateAPIs = privateAPIs;
            this._httpClient = new HttpClient();
        }

        public async Task DeleteUserDeviceAsync(int userDeviceId)
        {
            try
            {
                Log.Information("Processing in UserDeviceRepository to get stores.");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, this._privateAPIs.UserDeviceAPI + $"/{userDeviceId}");
                Log.Information("Processing in UserDeviceRepository to Call API from MBKC Private API");
                HttpResponseMessage response = await this._httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    List<Store> stores = JsonConvert.DeserializeObject<List<Store>>(responseText);
                    Log.Information("Deleting User Device successfully in UserDeviceRepository => Data: {Data}", responseText);
                }
                else
                {
                    throw new Exception("Accessing to MBKC Private API Failed in UserDeviceRepository Failed.");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in UserDeviceRepository => Exception: {Message}", ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
