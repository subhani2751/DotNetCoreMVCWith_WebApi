using DotNetCoreMVCWith_WebApi.Models;
using DotNetCoreMVCWith_WebApi.MyDatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreMVCWith_WebApi.Controllers
{
    
    public class EmployeeController : Controller
    {
        private readonly EmployeeDbContext _employeeDbContext;
        public EmployeeController(EmployeeDbContext employeeDbContext)
        {
            _employeeDbContext = employeeDbContext;
        }
        public async Task<IActionResult> Login(int partialviewneed=0)
        {
            if (partialviewneed == 0)
            {
                return View("Views/Login/Login.cshtml");
            }
            return PartialView("Views/Login/Login.cshtml");
        }
        public async Task<IActionResult> Register()
        {
            return View("Views/Login/Register.cshtml");
        }
        [Authorize]
        public async Task<IActionResult> AddEmployee()
        {
           //var Employeeslst= await _employeeDbContext.Employees.ToListAsync();
            //return View();
            return PartialView("Views/Employee/AddEmployee.cshtml");
        }
        [Authorize]
        public async Task<IActionResult> EditEmployee(int id=0)
        {
            var Employees = await _employeeDbContext.Employees.FindAsync(id);
            return View(Employees);
        }
    }
}
