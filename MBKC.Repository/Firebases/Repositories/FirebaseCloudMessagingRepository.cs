using FirebaseAdmin.Messaging;
using MBKC.Repository.Firebases.Models;
using MBKC.Repository.GrabFoods.Models;
using MBKC.Repository.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Apis.Requests.BatchRequest;

namespace MBKC.Repository.Firebases.Repositories
{
    public class FirebaseCloudMessagingRepository
    {
        public FirebaseCloudMessagingRepository()
        {

        }

        private FirebaseCloudMessaging GetFirebaseCloudMessaging()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                                  .SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            return new FirebaseCloudMessaging()
            {
                Logo = configuration.GetSection("FirebaseCloudMessaging:Logo").Value,
                ClickAction = configuration.GetSection("FirebaseCloudMessaging:ClickAction").Value,
                Screen = configuration.GetSection("FirebaseCloudMessaging:Screen").Value

            };
        }

        public async Task PushNotificationAsync(string title, string body, string fcmToken, int idOrder)
        {
            try
            {
                FirebaseCloudMessaging firebaseCloudMessaging = GetFirebaseCloudMessaging();
                Message message = new Message()
                {
                    Token = fcmToken,
                    Data = new Dictionary<string, string>()
                    {
                        { "title", title},
                        { "body", body },
                        { "click_action",  firebaseCloudMessaging.ClickAction},
                        { "screen", firebaseCloudMessaging.Screen },
                        { "orderid", $"{idOrder}" }
                    },
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body,
                        ImageUrl = firebaseCloudMessaging.Logo
                    },
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Log.Information("Successfully sent mesage: " + response);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
