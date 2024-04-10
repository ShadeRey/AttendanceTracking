using System;

namespace AttendanceTracking.Models;

public class ReportParameters
{
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}