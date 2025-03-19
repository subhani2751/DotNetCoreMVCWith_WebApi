using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using DotNetCoreMVCWith_WebApi.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DotNetCoreMVCWith_WebApi.Services
{
    public class Export_Import
    {
        public ActionResult ExportCSV(List<Employee> employees)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("EmployeeID,FirstName,LastName,Email,Department,HireDate,Salary");
            var hireDate = string.Empty; ;
            foreach (var emp in employees)
            {
                hireDate = emp.HireDate.HasValue ? emp.HireDate.Value.ToShortDateString() : "N/A";
                sb.AppendLine($"{emp.EmployeeId},{emp.FirstName},{emp.LastName},{emp.Email},{emp.Department},{hireDate},{emp.Salary}");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            return new FileContentResult(buffer, "text/csv") { FileDownloadName = "SelectedEmployees.csv" };
        }
        public ActionResult ExportExcel(List<Employee> employees)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Employees");
                worksheet.Cell(1, 1).Value = "EmployeeID";
                worksheet.Cell(1, 2).Value = "FirstName";
                worksheet.Cell(1, 3).Value = "LastName";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Department";
                worksheet.Cell(1, 6).Value = "HireDate";
                worksheet.Cell(1, 7).Value = "Salary";

                int row = 2;
                foreach (var emp in employees)
                {
                    worksheet.Cell(row, 1).Value = emp.EmployeeId;
                    worksheet.Cell(row, 2).Value = emp.FirstName;
                    worksheet.Cell(row, 3).Value = emp.LastName;
                    worksheet.Cell(row, 4).Value = emp.Email;
                    worksheet.Cell(row, 5).Value = emp.Department;
                    worksheet.Cell(row, 6).Value = emp.HireDate.HasValue ? emp.HireDate.Value.ToShortDateString() : "N/A";
                    worksheet.Cell(row, 7).Value = emp.Salary;
                    row++;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = "SelectedEmployees.xlsx" };
                }
            }
        }

        public ActionResult ExportPDF(List<Employee> employees)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Document doc = new Document();
                PdfWriter writer = PdfWriter.GetInstance(doc, stream);
                doc.Open();

                Font font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                PdfPTable table = new PdfPTable(7);
                table.AddCell(new PdfPCell(new Phrase("EmployeeID", font)));
                table.AddCell(new PdfPCell(new Phrase("First Name", font)));
                table.AddCell(new PdfPCell(new Phrase("Last Name", font)));
                table.AddCell(new PdfPCell(new Phrase("Email", font)));
                table.AddCell(new PdfPCell(new Phrase("Department", font)));
                table.AddCell(new PdfPCell(new Phrase("Hire Date", font)));
                table.AddCell(new PdfPCell(new Phrase("Salary", font)));

                foreach (var emp in employees)
                {
                    table.AddCell(emp.EmployeeId.ToString());
                    table.AddCell(emp.FirstName);
                    table.AddCell(emp.LastName);
                    table.AddCell(emp.Email);
                    table.AddCell(emp.Department);
                    table.AddCell(emp.HireDate.HasValue ? emp.HireDate.Value.ToShortDateString() : "N/A");
                    table.AddCell(emp.Salary.ToString());
                }

                doc.Add(table);
                doc.Close();

                return new FileContentResult(stream.ToArray(), "application/pdf") { FileDownloadName = "SelectedEmployees.pdf" };
            }
        }
    }
}
