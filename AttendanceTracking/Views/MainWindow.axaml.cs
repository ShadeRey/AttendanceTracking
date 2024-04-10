using System;
using System.Reactive.Linq;
using AttendanceTracking.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace AttendanceTracking.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        ViewModel = new MainWindowViewModel();
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ViewModel.WhenAnyValue(x => x.CaptchaVisible)
            .DistinctUntilChanged()
            .Subscribe(ViewModelOnCaptchaVisibilityChanged);
    }

    private void ViewModelOnCaptchaVisibilityChanged(bool obj)
    {
        CaptchaControl.Generate();
        if (ViewModel != null)
        {
            ViewModel.CaptchaText = CaptchaControl.CaptchaText;
        }
    }
}