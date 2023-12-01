using MBKC.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Interfaces
{
    public interface IUserDeviceService
    {
        public Task PushNotificationAsync(string title, string body, int idOrder, List<UserDevice> userDevices);
        public Task PushNotificationAsync_Tool(string title, string body, int idOrder, List<UserDevice> userDevices);
    }
}
