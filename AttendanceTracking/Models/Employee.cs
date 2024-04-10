using System;

namespace AttendanceTracking.Models;

public class Employee
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = String.Empty;
    public int Role { get; set; }
    public string Login { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
    public string RoleName { get; set; } = String.Empty;
}