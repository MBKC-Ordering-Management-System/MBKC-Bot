using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.RabbitMQs.Repositories
{
    public class RabbitMQRepository
    {  
        public RabbitMQRepository()
        {
         
        }

        private Models.RabbitMQ GetRabbitMQ()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                                  .SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            return new Models.RabbitMQ()
            {
                HostName = configuration.GetSection("RabbitMQ:HostName").Value,
                Username = configuration.GetSection("RabbitMQ:Username").Value,
                Password = configuration.GetSection("RabbitMQ:Password").Value,
                VirtualHost = configuration.GetSection("RabbitMQ:VirtualHost").Value,
                URI = configuration.GetSection("RabbitMQ:URI").Value,
            };
        }

        public string ReceiveMessage(string queueName)
        {
            try
            {
                Models.RabbitMQ rabbitMQ = GetRabbitMQ();
                ConnectionFactory connectionFactory = new ConnectionFactory()
                {
                    HostName = rabbitMQ.HostName,
                    UserName = rabbitMQ.Username,
                    Password = rabbitMQ.Password,
                    VirtualHost = rabbitMQ.VirtualHost,
                    Uri = new Uri(rabbitMQ.URI)
                };

                IConnection connection = connectionFactory.CreateConnection();

                using IModel channel = connection.CreateModel();
                channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                string message = "";
                consumer.Received += (sender, args) => {
                    byte[] body = args.Body.ToArray();
                    message = Encoding.UTF8.GetString(body);

                    channel.BasicAck(args.DeliveryTag, false);
                };

                string consumerTag = channel.BasicConsume(queueName, false, consumer);

                channel.BasicCancel(consumerTag);
                channel.Close();
                connection.Close();
                return message;
            } catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
