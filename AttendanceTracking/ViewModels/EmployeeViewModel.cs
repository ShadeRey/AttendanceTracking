using System;
using ReactiveUI;

namespace AttendanceTracking.ViewModels;

public class EmployeeViewModel: ViewModelBase
{
    private string? _fullName = String.Empty;

    public string? FullName
    {
        get => _fullName;
        set => this.RaiseAndSetIfChanged(ref _fullName, value);
    }
    
    private bool _employeeViewVisible = false;
    
    public bool EmployeeViewVisible
    {
        get => _employeeViewVisible;
        set => this.RaiseAndSetIfChanged(ref _employeeViewVisible, value);
    }
}