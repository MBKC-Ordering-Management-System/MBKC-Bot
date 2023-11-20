using MBKC.Repository.Infrastructures;
using MBKC.Repository.Models;
using MBKC.Service.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Implementations
{
    public class UserDeviceService : IUserDeviceService
    {
        private UnitOfWork _unitOfWork;
        public UserDeviceService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task PushNotificationAsync(string title, string body, int idOrder, List<UserDevice> userDevices)
        {
            try
            {
                bool isFinished = false;
                int count = 0;
                if (userDevices is not null && userDevices.Count > 0)
                {
                    do
                    {
                        int? userDeviceId = null;
                        try
                        { 
                            if (count == userDevices.Count)
                            {
                                isFinished = true;
                            }
                            foreach (var userDevice in userDevices)
                            {
                                userDeviceId = userDevice.UserDeviceId;
                                count++;
                                this._unitOfWork.FirebaseCloudMessagingRepository.PushNotification(title, body, userDevice.FCMToken, idOrder);
                            }
                        }
                        catch (AggregateException ex)
                        {
                            if (ex.Message.Contains("Requested entity was not found"))
                            {
                                // remove fcm token
                                await this._unitOfWork.UserDeviceRepository.DeleteUserDeviceAsync(idOrder);
                                Log.Information("Deleting User Device in UserDeviceService Successfully.");
                            }
                        }
                    } while (isFinished == false);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in UserDeviceService. Error: {Error}", ex.Message);
            }
        }
    }
}
