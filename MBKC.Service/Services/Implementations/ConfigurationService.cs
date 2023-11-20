using MBKC.Repository.Infrastructures;
using MBKC.Repository.Models;
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
    public class ConfigurationService : IConfigurationService
    {
        private UnitOfWork _unitOfWork;
        public ConfigurationService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<List<Configuration>> GetConfigurationsAsync()
        {
            try
            {
                Log.Information("Processing in ConfigurationService to get configurations.");
                List<Configuration> configurations = await this._unitOfWork.ConfigurationRepository.GetConfigurationsAsync();
                Log.Information("Getting configurations successfully in ConfigurationService => Data: {Data}", JsonConvert.SerializeObject(configurations));
                return configurations;
            }
            catch (Exception ex)
            {
                Log.Error("Error in ConfigurationServive => Exception: {Message}", ex.Message);
                return null;
            }
        }
    }
}
