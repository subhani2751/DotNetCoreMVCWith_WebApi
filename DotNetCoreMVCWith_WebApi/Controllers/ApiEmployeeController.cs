﻿using Azure.Core;
using DotNetCoreMVCWith_WebApi.Models;
using DotNetCoreMVCWith_WebApi.MyDatabaseContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using DocumentFormat.OpenXml.InkML;
using DotNetCoreMVCWith_WebApi.Services;
using Microsoft.Extensions.Caching.Memory;

namespace DotNetCoreMVCWith_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiEmployeeController : ControllerBase
    {
        private readonly EmployeeDbContext _employeeDbContext;
        private readonly CacheMemory _cacheMemory;
        public ApiEmployeeController(EmployeeDbContext employeeDbContext, CacheMemory cacheMemory)
        {
            _employeeDbContext = employeeDbContext;
            _cacheMemory = cacheMemory;
        }
        [HttpPost]
        public async Task<IActionResult> post([FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest("Data not recived");
            }
            employee.CreatedDate = DateTime.Now;
            employee.LastModifiedDate = DateTime.Now;
            _employeeDbContext.Employees.Add(employee);
            var _Return = await _employeeDbContext.SaveChangesAsync();
            employee.smessage = "Employee added successfully";
            return Ok(employee);
        }
        [HttpGet("GetAllEmployees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            string key = "GetAllEmployees";

            var LstEmployees = _cacheMemory.getcache<Employee>(key);
            if (LstEmployees == null)
            {
                 LstEmployees = await _employeeDbContext.Employees.ToListAsync();
                if (LstEmployees.Count > 0)
                {
                    _cacheMemory.setcache("GetAllEmployees", LstEmployees, Minutes: 5);
                }
            }
            return Ok(LstEmployees);
        }
        /*[HttpGet("Edit/{id}")]*/
        [HttpPut("Update")]
        public async Task<IActionResult> Update(Employee employee)
        {
            if (employee == null)
            {
                return BadRequest("Data not recived");
            }
            employee.LastModifiedDate = DateTime.Now;
            _employeeDbContext.Update(employee);
            var _Employees = await _employeeDbContext.SaveChangesAsync();
            return Ok(new { Message = "Employee Details modified successfully" });
        }
        [HttpPut("Delete")]
        public async Task<IActionResult> Delete(string ids)
        {
            var idslst = ids.Split(',')
                     .Select(id => Convert.ToInt32(id))
                     .ToList();
            foreach (var id in idslst)
            {
                var EmployeeDS = await _employeeDbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);
                _employeeDbContext.Remove(EmployeeDS);
            }
            var _Employees = await _employeeDbContext.SaveChangesAsync();
            _cacheMemory.Removecache("GetAllEmployees");
            return Ok(new { Message = "Employee Deleted successfully" });
        }
        [HttpPost("ImportExcel")]
        public async Task<IActionResult> ImportExcel(IFormFile excelFile)
        {
            try
            {
                if (excelFile == null || excelFile.Length == 0)
                    return BadRequest(new { success = false, message = "No file uploaded" });

                // Save the uploaded file temporarily
                var filePath = Path.GetTempFileName();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await excelFile.CopyToAsync(stream);
                }

                // Connection string for Excel
                string excelConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties='Excel 12.0;HDR=YES;'";

                // Create DataTable to hold Excel data
                DataTable dt = new DataTable();

                // Read from Excel
                using (var excelConn = new OleDbConnection(excelConnectionString))
                {
                    await excelConn.OpenAsync();
                    using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", excelConn))
                    {
                        using (OleDbDataAdapter da = new OleDbDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                // Clean up temp file
                System.IO.File.Delete(filePath);

                // Get the connection string from the DbContext
                string sqlConnectionString = _employeeDbContext.Database.GetDbConnection().ConnectionString;

                // Bulk insert using ADO.NET
                using (SqlConnection conn = new SqlConnection(sqlConnectionString))
                {
                    await conn.OpenAsync();

                    // Create SqlBulkCopy object
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                    {
                        bulkCopy.DestinationTableName = "Employees"; // Your table name

                        // Column mappings (adjust according to your Excel column names and DB columns)
                        bulkCopy.ColumnMappings.Add("FirstName", "FirstName");
                        bulkCopy.ColumnMappings.Add("LastName", "LastName");
                        bulkCopy.ColumnMappings.Add("Email", "Email");
                        bulkCopy.ColumnMappings.Add("Department", "Department");
                        bulkCopy.ColumnMappings.Add("HireDate", "HireDate");
                        bulkCopy.ColumnMappings.Add("Salary", "Salary");
                        bulkCopy.ColumnMappings.Add("CreatedDate", "CreatedDate"); // Added for completeness
                        bulkCopy.ColumnMappings.Add("LastModifiedDate", "LastModifiedDate"); // Added for completeness

                        // Set default values for CreatedDate and LastModifiedDate if not in Excel
                        if (!dt.Columns.Contains("CreatedDate"))
                        {
                            dt.Columns.Add("CreatedDate", typeof(DateTime));
                            foreach (DataRow row in dt.Rows)
                                row["CreatedDate"] = DateTime.Now;
                        }
                        if (!dt.Columns.Contains("LastModifiedDate"))
                        {
                            dt.Columns.Add("LastModifiedDate", typeof(DateTime));
                            foreach (DataRow row in dt.Rows)
                                row["LastModifiedDate"] = DateTime.Now;
                        }

                        // Write to database
                        await bulkCopy.WriteToServerAsync(dt);
                    }
                }

                return Ok(new { success = true, message = "Employees imported successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpPost("Export")]
        public async Task<IActionResult> Export(string format, string ids)
        {
            Export_Import export_Import = new Export_Import();
            if (ids == null || !ids.Any())
            {
                return Ok(new { success = false, message = "No employees selected." });
            }
            var idslst = ids.Split(',')
                    .Select(id => Convert.ToInt32(id))
                    .ToList();

            List<Employee> employees = await _employeeDbContext.Employees.Where(e => idslst.Contains(e.EmployeeId)).ToListAsync();

            switch (format)
            {
                case "CSV":
                    return export_Import.ExportCSV(employees);
                case "Excel":
                    return export_Import.ExportExcel(employees);
                case "PDF":
                    return export_Import.ExportPDF(employees);
                default:
                    return Ok(new { success = false, message = "Invalid export format." });
            }
        }

        [HttpGet("getTablePageData")]
        public async Task<IActionResult> getTablePageData(int pagesize =10 ,int pagenumber=1)
        {
            EmployeeApiResponse employeeApiResponse = new EmployeeApiResponse();
            var AllEmployees = _cacheMemory.getcache<Employee>("GetAllEmployees");
            if(AllEmployees != null)
            {
                employeeApiResponse.TotalRecords = AllEmployees.Count();
                employeeApiResponse.lstEmployees = AllEmployees.Skip((pagenumber-1)*pagesize).Take(pagesize).ToList();
            }
            else
            {
                AllEmployees = await _employeeDbContext.Employees.ToListAsync();
                employeeApiResponse.TotalRecords = AllEmployees.Count();
                employeeApiResponse.lstEmployees = AllEmployees.Skip((pagenumber - 1) * pagesize).Take(pagesize).ToList();
                if (AllEmployees.Count > 0)
                {
                    _cacheMemory.setcache("GetAllEmployees", AllEmployees, Minutes: 5);
                }
            }
               

            return Ok(employeeApiResponse);
        }
    }
}
