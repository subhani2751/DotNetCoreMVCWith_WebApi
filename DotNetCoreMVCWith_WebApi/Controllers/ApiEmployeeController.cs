using Azure.Core;
using DotNetCoreMVCWith_WebApi.Models;
using DotNetCoreMVCWith_WebApi.MyDatabaseContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreMVCWith_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiEmployeeController : ControllerBase
    {
        private readonly EmployeeDbContext _employeeDbContext;
        public ApiEmployeeController(EmployeeDbContext employeeDbContext)
        {
            _employeeDbContext=employeeDbContext;
        }
        [HttpPost]
        public async Task<IActionResult> post([FromBody] Employee employee)
        {
            if(employee == null)
            {
                return BadRequest("Data not recived");
            }
            employee.CreatedDate= DateTime.Now;
            employee.LastModifiedDate= DateTime.Now;
            _employeeDbContext.Employees.Add(employee);
            var _Return=await _employeeDbContext.SaveChangesAsync();
            employee.smessage = "Employee added successfully";
            return Ok(employee);
        }
        [HttpGet("GetAllEmployees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var LstEmployees=await _employeeDbContext.Employees.ToListAsync();
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
            if (employee!=null && employee.CreatedDate == null)
            {
                employee.CreatedDate = DateTime.Now;
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
    }
}
