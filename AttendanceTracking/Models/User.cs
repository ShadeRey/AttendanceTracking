using System;
using AttendanceTracking.ViewModels;
using ReactiveUI;

namespace AttendanceTracking.Models;

public class User: ViewModelBase
{
    private string _role;

    public string Role {
        get => _role;
        set => this.RaiseAndSetIfChanged(ref _role, value);
    }
}