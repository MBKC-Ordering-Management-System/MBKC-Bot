using MBKC.Repository.SMTPs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Repository.SMTPs.Repositories
{
    public class EmailRepository
    {
        public EmailRepository()
        {
            
        }

        private Email GetEmail()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                                  .SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            return new Email()
            {
                Host = configuration.GetSection("SMTP:Email:Host").Value,
                Port = int.Parse(configuration.GetSection("SMTP:Email:Port").Value),
                SystemName = configuration.GetSection("SMTP:Email:SystemName").Value,
                Sender = configuration.GetSection("SMTP:Email:Sender").Value,
                Password = configuration.GetSection("SMTP:Email:Password").Value,
            };
        }

        public string GetMessageToNotifyNonMappingOrder(string receiverEmail, string messageBody)
        {
            Email email = GetEmail();
            string emailBody = "";
            string htmlParentDivStart = "<div style=\"font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2\">";
            string htmlParentDivEnd = "</div>";
            string htmlMainDivStart = "<div style=\"margin:50px auto;width:70%;padding:20px 0\">";
            string htmlMainDivEnd = "</div>";
            string htmlSystemNameDivStart = "<div style=\"border-bottom:1px solid #eee\">";
            string htmlSystemNameDivEnd = "</div";
            string htmlSystemNameSpanStart = "<span style=\"font-size:1.4em;color: #00466a;text-decoration:none;font-weight:600\">";
            string htmlSystemNameSpanEnd = "</span>";
            string htmlHeaderBodyStart = "<p style=\"font-size:1.1em\">";
            string htmlHeaderBodyEnd = "</p>";
            string htmlBodyStart = "<p>";
            string htmlBodyEnd = "</p>";
            string htmlFooterBodyStart = "<p style=\"font-size:0.9em;\">";
            string htmlBreakLine = "<br />";
            string htmlFooterBodyEnd = "</p>";

            emailBody += htmlParentDivStart;
            emailBody += htmlMainDivStart;
            emailBody += htmlSystemNameDivStart + htmlSystemNameSpanStart + email.SystemName + htmlSystemNameSpanEnd + htmlSystemNameDivEnd + htmlBreakLine;
            emailBody += htmlHeaderBodyStart + $"Hi {receiverEmail}," + htmlHeaderBodyEnd;
            emailBody += htmlBodyStart + messageBody + htmlBodyEnd;
            emailBody += htmlFooterBodyStart + "Regards," + htmlBreakLine + email.SystemName + htmlFooterBodyEnd;
            emailBody += htmlMainDivEnd;
            emailBody += htmlParentDivEnd;

            return emailBody;
        }


        public async Task SendEmailToNotifyNonMappingOrder(string receiverEmail, string message, Attachment attachment)
        {
            try
            {
                Email email = GetEmail();
                MailMessage mailMessage = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                mailMessage.From = new MailAddress(email.Sender);
                mailMessage.To.Add(new MailAddress(receiverEmail));
                mailMessage.Subject = $"Non-Mapping Orders To MBKC System [{DateTime.Now.Day}/{DateTime.Now.Month}/{DateTime.Now.Year}]";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = message;
                mailMessage.Attachments.Add(attachment);
                smtp.Port = email.Port;
                smtp.Host = email.Host;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(email.Sender, email.Password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                await smtp.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
