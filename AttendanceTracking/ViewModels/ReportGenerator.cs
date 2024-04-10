using System;
using System.Collections.Generic;
using AttendanceTracking.Models;

namespace AttendanceTracking.ViewModels;

public class ReportGenerator
{
    private readonly List<Visit> _visits;

    public ReportGenerator(List<Visit> visits)
    {
        _visits = visits;
    }

    public Dictionary<string, int> GenerateReport(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        Dictionary<string, int> visitCounts = new Dictionary<string, int>();

        foreach (Visit visit in _visits)
        {
            if (visit.Date >= startDate && visit.Date <= endDate)
            {
                string clientName = visit.ClientName;

                if (!visitCounts.ContainsKey(clientName))
                {
                    visitCounts[clientName] = 1;
                }
                else
                {
                    visitCounts[clientName]++;
                }
            }
        }

        return visitCounts;
    }
}