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
    public class RabbitMQService: IRabbitMQService
    {
        private UnitOfWork _unitOfWork;
        public RabbitMQService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
        }

        public Configuration ReceiveMessage(string queueName)
        {
            try
            {
                Log.Information("Processing receive message from RabbitMQ in RabbitMQService.");
                string jsonMessage = this._unitOfWork.RabbitMQRepository.ReceiveMessage(queueName);
                Configuration configuration = JsonConvert.DeserializeObject<Configuration>(jsonMessage);
                return configuration;
            } catch(Exception ex)
            {
                Log.Error("Error in RabbitMQService. Error: {Error}", ex.Message);
                return null;
            }
        }
    }
}
