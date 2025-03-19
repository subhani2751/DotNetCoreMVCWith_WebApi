using Azure.Core;
using DotNetCoreMVCWith_WebApi.Models;
using DotNetCoreMVCWith_WebApi.MyDatabaseContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;

namespace DotNetCoreMVCWith_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiEmployeeController : ControllerBase
    {
        private readonly EmployeeDbContext _employeeDbContext;
        public ApiEmployeeController(EmployeeDbContext employeeDbContext)
        {
            _employeeDbContext = employeeDbContext;
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
            var LstEmployees = await _employeeDbContext.Employees.ToListAsync();
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
    }
}
