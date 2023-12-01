using MBKC.Repository.Infrastructures;
using MBKC.Service.DTOs.Orders;
using MBKC.Service.Services.Interfaces;
using MBKC.Service.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Services.Implementations
{
    public class EmailService: IEmailService
    {
        private UnitOfWork _unitOfWork;
        public EmailService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task SendEmailForFailedOrderAsync(List<FailedOrder> failedOrders, string receiverEmail)
        {
            try
            {
                Log.Information("Processing to send Email in EmailService.");
                DataTable failedOrdersDT = ExcelUtil.GetFailedOrders(failedOrders);
                Attachment failedOrdersAttachment = ExcelUtil.GetAttachmentForFailedOrders(failedOrdersDT);

                string message = "MBKC system has received orders from the Partner systems that you have joined. " +
                    "The MBKC system is currently unable to map some orders in the Excel attachment below to the system. " +
                    "Please review the products so that the order mapping process can become better.";
                string messageBody = this._unitOfWork.EmailRepository.GetMessageToNotifyNonMappingOrder(receiverEmail, message);
                await this._unitOfWork.EmailRepository.SendEmailToNotifyNonMappingOrder(receiverEmail, messageBody, failedOrdersAttachment);
                Log.Information("Sent Email Successfully in EmailService.");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Sent Email about Failed Orders Successfully.");
                Console.ResetColor();
            } catch(Exception ex)
            {
                Log.Error("Error in StoreService => Exception: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Sent Email about Failed Orders that is failed.");
                Console.ResetColor();
            }
        }

        public async Task SendEmailForFailedOrderAsync_Tool(List<FailedOrder> failedOrders, string receiverEmail)
        {
            try
            {
                Log.Information("Processing to send Email in EmailService.");
                DataTable failedOrdersDT = ExcelUtil.GetFailedOrders(failedOrders);
                Attachment failedOrdersAttachment = ExcelUtil.GetAttachmentForFailedOrders(failedOrdersDT);

                string message = "MBKC system has received orders from the Partner systems that you have joined. " +
                    "The MBKC system is currently unable to map some orders in the Excel attachment below to the system. " +
                    "Please review the products so that the order mapping process can become better.";
                string messageBody = this._unitOfWork.EmailRepository.GetMessageToNotifyNonMappingOrder(receiverEmail, message);
                await this._unitOfWork.EmailRepository.SendEmailToNotifyNonMappingOrder(receiverEmail, messageBody, failedOrdersAttachment);
                Log.Information("Sent Email Successfully in EmailService.");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Sent Email about Failed Orders Successfully.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Log.Error("Error in StoreService => Exception: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Sent Email about Failed Orders that is failed.");
                Console.ResetColor();
                throw new Exception(ex.Message);
            }
        }
    }
}
