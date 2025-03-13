using Microsoft.AspNetCore.Mvc;

namespace DotNetCoreMVCWith_WebApi.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult AddEmployee()
        {
            return View();
        }
        public IActionResult EditEmployee()
        {
            return View();
        }
    }
}
