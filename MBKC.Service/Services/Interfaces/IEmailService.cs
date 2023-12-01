using MBKC.Service.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Interfaces
{
    public interface IEmailService
    {
        public Task SendEmailForFailedOrderAsync(List<FailedOrder> failedOrders, string receiverName);
        public Task SendEmailForFailedOrderAsync_Tool(List<FailedOrder> failedOrders, string receiverName);
    }
}
