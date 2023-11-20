using MBKC.Repository.Firebases.Repositories;
using MBKC.Repository.GrabFoods.Repositories;
using MBKC.Repository.MBKCs.Models;
using MBKC.Repository.RabbitMQs.Repositories;
using MBKC.Repository.Repositories;
using MBKC.Repository.SMTPs.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.Infrastructures
{
    public class UnitOfWork: IUnitOfWork
    {
        private ConfigurationRepository _configurationRepository;
        private StoreRepository _storeRepository;
        private GrabFoodRepository _grabFoodRepository;
        private EmailRepository _emailRepository;
        private RabbitMQRepository _rabbitMQRepository;
        private OrderRepository _orderRepository;
        private FirebaseCloudMessagingRepository _firebaseCloudMessagingRepository;
        private UserDeviceRepository _userDeviceRepository;
        public UnitOfWork()
        {
            
        }

        private PrivateAPIs GetPrivateAPIs()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                                  .SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            return new PrivateAPIs()
            {
                ConfigurationAPI = configuration.GetSection("MBKC:PrivateAPIs:ConfigurationAPI").Value,
                StoreAPI = configuration.GetSection("MBKC:PrivateAPIs:StoreAPI").Value,
                OrderAPI = configuration.GetSection("MBKC:PrivateAPIs:OrderAPI").Value,
                UserDeviceAPI = configuration.GetSection("MBKC:PrivateAPIs:UserDeviceAPI").Value
            };
        }


        public ConfigurationRepository ConfigurationRepository
        {
            get
            {
                if(this._configurationRepository == null)
                {
                    PrivateAPIs privateAPIs = GetPrivateAPIs();
                    this._configurationRepository = new ConfigurationRepository(privateAPIs);
                }
                return this._configurationRepository;
            }
        }

        public StoreRepository StoreRepository
        {
            get
            {
                if(this._storeRepository == null)
                {
                    PrivateAPIs privateAPIs = GetPrivateAPIs();
                    this._storeRepository = new StoreRepository(privateAPIs);
                }
                return this._storeRepository;
            }
        }

        public GrabFoodRepository GrabFoodRepository
        {
            get
            {
                if(this._grabFoodRepository == null)
                {
                    this._grabFoodRepository = new GrabFoodRepository();
                }
                return this._grabFoodRepository;
            }
        }
        
        public EmailRepository EmailRepository
        {
            get
            {
                if(this._emailRepository == null)
                {
                    this._emailRepository = new EmailRepository();
                }
                return this._emailRepository;
            }
        }

        public RabbitMQRepository RabbitMQRepository
        {
            get
            {
                if(this._rabbitMQRepository == null)
                {
                    this._rabbitMQRepository = new RabbitMQRepository();
                }
                return this._rabbitMQRepository;
            }
        }

        public OrderRepository OrderRepository
        {
            get
            {
                if(this._orderRepository == null)
                {
                    PrivateAPIs privateAPIs = GetPrivateAPIs();
                    this._orderRepository = new OrderRepository(privateAPIs);
                }
                return this._orderRepository;
            }
        }
        
        public FirebaseCloudMessagingRepository FirebaseCloudMessagingRepository
        {
            get
            {
                if(this._firebaseCloudMessagingRepository == null)
                {
                    this._firebaseCloudMessagingRepository = new FirebaseCloudMessagingRepository();
                }
                return this._firebaseCloudMessagingRepository;
            }
        }

        public UserDeviceRepository UserDeviceRepository
        {
            get
            {
                if(this._userDeviceRepository == null)
                {
                    PrivateAPIs privateAPIs = GetPrivateAPIs();
                    this._userDeviceRepository = new UserDeviceRepository(privateAPIs);
                }
                return this._userDeviceRepository;
            }
        }
    }
}
