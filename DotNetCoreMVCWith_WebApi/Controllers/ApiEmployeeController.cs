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
using DocumentFormat.OpenXml.InkML;
using DotNetCoreMVCWith_WebApi.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity;
using System.Drawing;
using System.Security.Claims;

namespace DotNetCoreMVCWith_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiEmployeeController : ControllerBase
    {
        private readonly EmployeeDbContext _employeeDbContext;
        private readonly CacheMemory _cacheMemory;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _config;
        public ApiEmployeeController(EmployeeDbContext employeeDbContext, CacheMemory cacheMemory, IPasswordHasher<User> passwordHasher,
        IConfiguration config)
        {
            _employeeDbContext = employeeDbContext;
            _cacheMemory = cacheMemory;
            _passwordHasher = passwordHasher;
            _config = config;
        }
        [HttpPost]
        public async Task<IActionResult> post([FromForm]  Employee employee)//[FromBody]
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
            _cacheMemory.Removecache("GetAllEmployees");
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
            AdoDataAdapter adoDataAdapter = new AdoDataAdapter(_config);
            List<Employee> tabledata =adoDataAdapter.Getallemployees().AsEnumerable().Select(r=> new Employee { FirstName =r.Field<string>("FirstName")}).ToList();
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

        [HttpGet("CheckCredentials")]
        public async Task<IActionResult> CheckCredentials(string Username = "", string PasswordHash = "")
        {
            TokenJWT tokenJWT = new TokenJWT(_config);
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(PasswordHash))
                return BadRequest("Username and password are required.");

            var user = await _employeeDbContext.Users.FirstOrDefaultAsync(u => u.Username == Username);
            if (user == null)
                return Unauthorized("Invalid username or password");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, PasswordHash);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid username or password");

            if (user.IsEmailConfirmed == false)
                return Unauthorized("Email not confirmed.");

            var token = tokenJWT.GenerateJwtToken(user); // 👇 defined below

            return Ok(new
            {
                Message = "Login successful",
                Token = token,
                Username = user.Username,
                Role = user.Role
            });
        }

        [HttpPost("RegisterCredentials")]
        public async Task<IActionResult> RegisterCredentials(string Username="",string Email="",string PasswordHash="")
        {
            // 1. Validate model
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2. Check if user already exists
            var existingUser = await _employeeDbContext.Users.FirstOrDefaultAsync(u => u.Username == Username || u.Email == Email);
            if (existingUser != null)
                return BadRequest(new { Message = "Username or Email already exists." });

            // 3. Create user and hash password
            var user = new User
            {
                Username = Username,
                Email = Email,
                // other properties like CreatedAt, Role, etc.
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, PasswordHash);

            // 4. Save to database
            _employeeDbContext.Users.Add(user);
            await _employeeDbContext.SaveChangesAsync();

            // 5. (Optional) Generate JWT and return it
            // var token = GenerateJwtToken(user);
            // return Ok(new { Message = "Registered", Token = token });

            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null || !identity.IsAuthenticated)
                return Unauthorized();

            var username = identity.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            // Fetch user from DB (optional, for role/email/etc)
            var user = _employeeDbContext.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return Unauthorized();
            TokenJWT tokenJWT = new TokenJWT(_config);
            // Generate new token (2 minutes expiry)
            var token = tokenJWT.GenerateJwtToken(user); // Use your JWT generator service

            return Ok(new { token });
        }

    }
}
