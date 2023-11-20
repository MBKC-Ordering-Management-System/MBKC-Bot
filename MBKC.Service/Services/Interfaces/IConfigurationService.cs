using MBKC.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Interfaces
{
    public interface IConfigurationService
    {
        public Task<List<Configuration>> GetConfigurationsAsync();
    }
}
