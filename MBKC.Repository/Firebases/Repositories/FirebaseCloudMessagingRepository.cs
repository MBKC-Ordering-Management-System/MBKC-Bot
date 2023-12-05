using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
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
            FileInfo fileInfo = new FileInfo("admin_sdk.json");
            string fullPath = fileInfo.FullName;
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(fileInfo.FullName)
            });
        }

        private FirebaseCloudMessaging GetFirebaseCloudMessaging()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                                  .SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            FirebaseCloudMessaging firebaseCloudMessaging = new FirebaseCloudMessaging()
            {
                Logo = configuration.GetSection("FirebaseCloudMessaging:Logo").Value,
                ClickAction = configuration.GetSection("FirebaseCloudMessaging:ClickAction").Value,
                Screen = configuration.GetSection("FirebaseCloudMessaging:Screen").Value

            };
            return firebaseCloudMessaging;
        }

        public void PushNotification(string title, string body, string fcmToken, int idOrder)
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

                string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
                Log.Information("Successfully sent mesage: " + response);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
