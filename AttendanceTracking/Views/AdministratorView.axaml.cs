using System;
using System.Collections.Generic;
using System.Linq;
using AttendanceTracking.Models;
using AttendanceTracking.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ClosedXML.Excel;

namespace AttendanceTracking.Views;

public partial class AdministratorView : UserControl
{
    public AdministratorView()
    {
        InitializeComponent();
    }

    public AdministratorViewModel ViewModel => (DataContext as AdministratorViewModel)!;
    
    private void VisitSearch_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (ViewModel.VisitsPreSearch is null)
        {
            ViewModel.VisitsPreSearch = ViewModel.Visit;
        }

        if (VisitSearch.Text is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(VisitSearch.Text))
        {
            VisitGrid.ItemsSource = ViewModel.VisitsPreSearch;
            return;
        }

        Filter();
    }

    private void Filter()
    {
        if (VisitSearch.Text is null)
        {
            return;
        }
        else
        {
            if (VisitFilter.SelectedIndex == 0)
            {
                var filtered = ViewModel.VisitsPreSearch.Where(
                    it => it.Id.ToString().Contains(VisitSearch.Text)
                          || it.Date.ToString().Contains(VisitSearch.Text)
                          || it.Time.ToString().Contains(VisitSearch.Text)
                          || it.Purpose.ToLower().Contains(VisitSearch.Text)
                          || it.ClientName.ToLower().Contains(VisitSearch.Text)
                          || it.EmployeeName.ToString().Contains(VisitSearch.Text)
                ).ToList();
                filtered = filtered.OrderBy(id => id.Id).ToList();
                VisitGrid.ItemsSource = filtered;
            }
            else if (VisitFilter.SelectedIndex == 1)
            {
                var filtered = ViewModel.VisitsPreSearch
                    .Where(it => it.ClientName.Contains(VisitSearch.Text)).ToList();
                filtered = filtered.OrderBy(clientName => clientName.ClientName).ToList();
                VisitGrid.ItemsSource = filtered;
            }
            else if (VisitFilter.SelectedIndex == 2)
            {
                var filtered = ViewModel.VisitsPreSearch
                    .Where(it => it.EmployeeName.Contains(VisitSearch.Text)).ToList();
                filtered = filtered.OrderBy(employeeName => employeeName.EmployeeName).ToList();
                VisitGrid.ItemsSource = filtered;
            }
            else if (VisitFilter.SelectedIndex == 3)
            {
                var filtered = ViewModel.VisitsPreSearch
                    .Where(it => it.Date.ToString().Contains(VisitSearch.Text)).ToList();
                filtered = filtered.OrderBy(date => date.Date).ToList();
                VisitGrid.ItemsSource = filtered;
            }
            else if (VisitFilter.SelectedIndex == 4)
            {
                var filtered = ViewModel.VisitsPreSearch
                    .Where(it => it.Time.ToString().Contains(VisitSearch.Text)).ToList();
                filtered = filtered.OrderBy(time => time.Time).ToList();
                VisitGrid.ItemsSource = filtered;
            }
            else if (VisitFilter.SelectedIndex == 5)
            {
                var filtered = ViewModel.VisitsPreSearch
                    .Where(it => it.Purpose.Contains(VisitSearch.Text)).ToList();
                filtered = filtered.OrderBy(purpose => purpose.Purpose).ToList();
                VisitGrid.ItemsSource = filtered;
            }
        }
    }

    private void VisitFilter_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => Filter();
}