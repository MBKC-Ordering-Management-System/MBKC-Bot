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
    public class ConfigurationRepository
    {
        private PrivateAPIs _privateAPIs;
        private HttpClient _httpClient;
        public ConfigurationRepository(PrivateAPIs privateAPIs)
        {
            this._privateAPIs = privateAPIs;
            this._httpClient = new HttpClient();
        }

        public async Task<List<Configuration>> GetConfigurationsAsync()
        {
            try
            {
                Log.Information("Processing in ConfigurationRepository to get configurations.");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, this._privateAPIs.ConfigurationAPI);
                Log.Information("Processing in ConfigurationRepository to Call API from MBKC Private API");
                HttpResponseMessage response = await this._httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    List<Configuration> configurations = JsonConvert.DeserializeObject<List<Configuration>>(responseText);
                    Log.Information("Getting configurations successfully in ConfigurationRepository => Data: {Data}", responseText);
                    return configurations;
                } else
                {
                    throw new Exception("Accessing to MBKC Private API Failed in ConfigurationRepository Failed.");
                }
            } catch(Exception ex)
            {
                Log.Error("Error in ConfigurationRepository => Exception: {Message}", ex.Message);
                throw new Exception(ex.Message);
            }
        } 
    }
}
