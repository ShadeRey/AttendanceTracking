using System;

namespace AttendanceTracking.Models;

public class Visit
{
    public int Id { get; set; }
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public TimeSpan Time { get; set; }
    public string Purpose { get; set; } = String.Empty;
    public int Client { get; set; }
    public int Employee { get; set; }
    public string ClientName { get; set; } = String.Empty;
    public string EmployeeName { get; set; } = String.Empty;
}