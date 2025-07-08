using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreMVCWith_WebApi.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Department { get; set; }

    public DateTime? HireDate { get; set; }

    public decimal? Salary { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastModifiedDate { get; set; }
    [NotMapped]
    public string? smessage { get; set; }
    [NotMapped]
    public int RecordCount { get; set; }

    [NotMapped]
    public List<IFormFile> Files { get; set; } = new();
}

public class EmployeeApiResponse
{
    public int TotalRecords { get; set; }
    public List<Employee> lstEmployees { get; set; }
}
