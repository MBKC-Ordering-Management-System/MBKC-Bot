using MBKC.Repository.GrabFoods.Models;
using MBKC.Repository.Models;
using MBKC.Service.DTOs.Orders;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.Service.Utils
{
    public static class ExcelUtil
    {
        public static DataTable GetFailedOrders(List<FailedOrder> failedOrders)
        {
            DataTable dataTable = new DataTable();
            dataTable.TableName = "Failed Orders";
            dataTable.Columns.Add("Order Id", typeof(string));
            dataTable.Columns.Add("Partner", typeof(string));
            dataTable.Columns.Add("Reason", typeof(string));

            failedOrders.ForEach(order =>
            {
                dataTable.Rows.Add(order.OrderId, order.PartnerName, order.Reason);
            });

            return dataTable;
        }


        public static Attachment GetAttachmentForFailedOrders(DataTable failedOrders)
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                MemoryStream outputStream = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(outputStream))
                {
                    ExcelWorksheet grabFoodItemsWorksheet = package.Workbook.Worksheets.Add("Failed Orders");
                    grabFoodItemsWorksheet.Cells.LoadFromDataTable(failedOrders, true);

                    package.Save();
                }

                outputStream.Position = 0;
                Attachment attachment = new Attachment(outputStream, "Failed_Orders.xlsx", "application/vnd.ms-excel");
                return attachment;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
