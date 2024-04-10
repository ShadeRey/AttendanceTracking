using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AttendanceTracking.DataBase;
using AttendanceTracking.Models;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using ClosedXML.Excel;
using DynamicData;
using MySqlConnector;
using ReactiveUI;
using SukiUI.Controls;

namespace AttendanceTracking.ViewModels;

public class AdministratorViewModel : ViewModelBase
{
    public AdministratorViewModel()
    {
        Visit = GetVisitsFromDb();
        Employee = GetEmployeesFromDb();
        _visit = GetVisitsFromDb();
    }
    
    private string? _fullName = String.Empty;

    public string? FullName
    {
        get => _fullName;
        set => this.RaiseAndSetIfChanged(ref _fullName, value);
    }
    
    private bool _administratorViewVisible = false;
    
    public bool AdministratorViewVisible
    {
        get => _administratorViewVisible;
        set => this.RaiseAndSetIfChanged(ref _administratorViewVisible, value);
    }
    
    private static readonly string ConnectionString = DataBaseConnectionString.ConnectionString;

    private AvaloniaList<Visit> GetVisitsFromDb()
    {
        AvaloniaList<Visit> visits = new AvaloniaList<Visit>();

        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            try
            {
                connection.Open();
                string selectAllVisits = """
                                         SELECT visit.Id, Date, Time, Purpose, Client, Employee, ClientName, EmployeeName From visit
                                         join client on visit.Client = client.Id
                                         join employee on visit.Employee = employee.Id
                                         """;
                MySqlCommand cmd = new MySqlCommand(selectAllVisits, connection);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Visit visitsItem = new Visit();
                    if (!reader.IsDBNull(reader.GetOrdinal("Id")))
                    {
                        visitsItem.Id = reader.GetInt32("Id");
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("Date")))
                    {
                        visitsItem.Date = reader.GetDateTimeOffset("Date");
                    }
                    
                    if (!reader.IsDBNull(reader.GetOrdinal("Time")))
                    {
                        visitsItem.Time = reader.GetTimeSpan("Time");
                    }
                    
                    if (!reader.IsDBNull(reader.GetOrdinal("Purpose")))
                    {
                        visitsItem.Purpose = reader.GetString("Purpose");
                    }
                    
                    if (!reader.IsDBNull(reader.GetOrdinal("Client")))
                    {
                        visitsItem.Client = reader.GetInt32("Client");
                    }
                    
                    if (!reader.IsDBNull(reader.GetOrdinal("Employee")))
                    {
                        visitsItem.Employee = reader.GetInt32("Employee");
                    }
                    
                    if (!reader.IsDBNull(reader.GetOrdinal("ClientName")))
                    {
                        visitsItem.ClientName = reader.GetString("ClientName");
                    }
                    
                    if (!reader.IsDBNull(reader.GetOrdinal("EmployeeName")))
                    {
                        visitsItem.EmployeeName = reader.GetString("EmployeeName");
                    }

                    visits.Add(visitsItem);
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Ошибка подключения к БД: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        return visits;
    }
    
    private AvaloniaList<Visit> _visit;

    public AvaloniaList<Visit> Visit
    {
        get => _visit;
        set => this.RaiseAndSetIfChanged(ref _visit, value);
    }
    
    public void OnNew(Visit visit) {
        Visit.Add(visit);
    }
    
    private Visit _visitSelectedItem;

    public Visit VisitSelectedItem {
        get => _visitSelectedItem;
        set => this.RaiseAndSetIfChanged(ref _visitSelectedItem, value);
    }
    
    public void OnDelete(Visit visit) {
        Visit.Remove(visit);
    }
    
    public void OnEdit(Visit visit) {
        Visit.Replace(VisitSelectedItem, visit);
    }
    
    private AvaloniaList<Visit> _visitsPreSearch;

    public AvaloniaList<Visit> VisitsPreSearch
    {
        get => _visitsPreSearch;
        set => this.RaiseAndSetIfChanged(ref _visitsPreSearch, value);
    }

    public void AddVisitToDB()
    {
        var db = new DataBaseAdd();

        var visit = new Visit();

        var clients = new List<Client>();
        {
            using var connection = new MySqlConnection(DataBaseConnectionString.ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("""
                                             select * from Client;
                                             """, connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read() && reader.HasRows)
            {
                var item = new Client()
                {
                    Id = reader.GetInt32("Id"),
                    ClientName = reader.GetString("ClientName")
                };
                clients.Add(item);
            }
        }
        
        var employees = new List<Employee>();
        {
            using var connection = new MySqlConnection(DataBaseConnectionString.ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("""
                                             select * from Employee;
                                             """, connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read() && reader.HasRows)
            {
                var item = new Employee()
                {
                    Id = reader.GetInt32("Id"),
                    EmployeeName = reader.GetString("EmployeeName")
                };
                employees.Add(item);
            }
        }
        
        var add = ReactiveCommand.Create((Visit i) =>
        {
            var newId = db.InsertData(
                "Visit",
                new MySqlParameter("@Date", MySqlDbType.DateTime)
                {
                    Value = i.Date
                },
                new MySqlParameter("@Time", MySqlDbType.Time)
                {
                    Value = i.Time
                },
                new MySqlParameter("@Purpose", MySqlDbType.VarChar)
                {
                    Value = i.Purpose
                },
                new MySqlParameter("@Client", MySqlDbType.Int32)
                {
                    Value = i.Client
                },
                new MySqlParameter("@Employee", MySqlDbType.Int32)
                {
                    Value = i.Employee
                }
            );
            i.Id = newId;
            i.ClientName = clients.FirstOrDefault(x => x.Id == i.Client).ClientName;
            i.EmployeeName = employees.FirstOrDefault(x => x.Id == i.Employee).EmployeeName;
            OnNew(i);
        });

        SukiHost.ShowDialog(new StackPanel()
        {
            DataContext = new Visit(),
            Children =
            {
                new DatePicker()
                {
                    SelectedDate = DateTimeOffset.Now,
                    [!DatePicker.SelectedDateProperty] = new Binding("Date")
                },
                new TimePicker()
                {
                    SelectedTime = DateTime.Now.TimeOfDay,
                    [!TimePicker.SelectedTimeProperty] = new Binding("Time")
                },
                new TextBox()
                {
                    Watermark = "Причина",
                    [!TextBox.TextProperty] = new Binding("Purpose")
                },
                new ComboBox()
                {
                    PlaceholderText = "Client",
                    ItemsSource = clients,
                    Name = "ClientComboBox",
                    DisplayMemberBinding = new Binding("ClientName"),
                    [!ComboBox.SelectedValueProperty] = new Binding("Client"),
                    SelectedValueBinding = new Binding("Id")
                },
                new ComboBox()
                {
                    PlaceholderText = "Employee",
                    ItemsSource = employees,
                    Name = "EmployeeComboBox",
                    DisplayMemberBinding = new Binding("EmployeeName"),
                    [!ComboBox.SelectedValueProperty] = new Binding("Employee"),
                    SelectedValueBinding = new Binding("Id")
                },
                new Button()
                {
                    Content = "Добавить",
                    Classes = { "Primary" },
                    Command = add,
                    Foreground = Brushes.White,
                    [!Button.CommandParameterProperty] = new Binding(".")
                },
                new Button()
                {
                    Content = "Закрыть",
                    Command = ReactiveCommand.Create(SukiHost.CloseDialog)
                }
            }
        }, allowBackgroundClose: true);
    }

    public void EditVisitInDB()
    {
        var db = new DataBaseEdit();
        int visitId = VisitSelectedItem.Id;
        var clients = new List<Client>();
        {
            using var connection = new MySqlConnection(DataBaseConnectionString.ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("""
                                             select * from Client;
                                             """, connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read() && reader.HasRows)
            {
                var item = new Client()
                {
                    Id = reader.GetInt32("Id"),
                    ClientName = reader.GetString("ClientName")
                };
                clients.Add(item);
            }
        }
        var employees = new List<Employee>();
        {
            using var connection = new MySqlConnection(DataBaseConnectionString.ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("""
                                             select * from Employee;
                                             """, connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read() && reader.HasRows)
            {
                var item = new Employee()
                {
                    Id = reader.GetInt32("Id"),
                    EmployeeName = reader.GetString("EmployeeName")
                };
                employees.Add(item);
            }
        }
        
        var edit = ReactiveCommand.Create<Visit>((i) =>
        {
            i.ClientName = clients.FirstOrDefault(x => x.Id == i.Client).ClientName;
            i.EmployeeName = employees.FirstOrDefault(x => x.Id == i.Employee).EmployeeName;
            db.EditData(
                "Visit",
                visitId,
                new MySqlParameter("@Date", MySqlDbType.DateTime)
                {
                    Value = i.Date
                },
                new MySqlParameter("Time", MySqlDbType.Time)
                {
                    Value = i.Time
                },
                new MySqlParameter("Purpose", MySqlDbType.VarChar)
                {
                    Value = i.Purpose
                },
                new MySqlParameter("Client", MySqlDbType.Int32)
                {
                    Value = i.Client
                },
                new MySqlParameter("Employee", MySqlDbType.Int32)
                {
                    Value = i.Employee
                }
            );
            OnEdit(i);
            SukiHost.CloseDialog();
        });

        var dataContext = new Visit()
        {
            Id = VisitSelectedItem.Id,
            Date = VisitSelectedItem.Date,
            Time = VisitSelectedItem.Time,
            Purpose = VisitSelectedItem.Purpose,
            Client = VisitSelectedItem.Client,
            Employee = VisitSelectedItem.Employee,
        };
        SukiHost.ShowDialog(new StackPanel()
        {
            DataContext = dataContext,
            Children =
            {
                new DatePicker()
                {
                    [!DatePicker.SelectedDateProperty] = new Binding("Date")
                },
                new TimePicker()
                {
                    [!TimePicker.SelectedTimeProperty] = new Binding("Time")
                },
                new TextBox()
                {
                    [!TextBox.TextProperty] = new Binding("Purpose")
                },
                new ComboBox()
                {
                    ItemsSource = clients,
                    Name = "ClientComboBox",
                    DisplayMemberBinding = new Binding("ClientName"),
                    [!ComboBox.SelectedValueProperty] = new Binding("Client"),
                    SelectedValueBinding = new Binding("Id")
                },
                new ComboBox()
                {
                    ItemsSource = employees,
                    Name = "EmployeeComboBox",
                    DisplayMemberBinding = new Binding("EmployeeName"),
                    [!ComboBox.SelectedValueProperty] = new Binding("Employee"),
                    SelectedValueBinding = new Binding("Id")
                },
                new Button()
                {
                    Content = "Обновить",
                    Classes = { "Primary" },
                    Command = edit,
                    Foreground = Brushes.White,
                    [!Button.CommandParameterProperty] = new Binding(".")
                },
                new Button()
                {
                    Content = "Закрыть",
                    Command = ReactiveCommand.Create(SukiHost.CloseDialog)
                }
            }
        }, allowBackgroundClose: true);
    }

    public void DeleteVisitFromDB()
    {
        if (VisitSelectedItem is null)
        {
            return;
        }
        var db = new DataBaseDelete();
        int visitId = VisitSelectedItem.Id;
        var delete = ReactiveCommand.Create<Visit>((i) =>
        {
            db.DeleteData(
                "Visit",
                visitId
            );
            OnDelete(i);
            SukiHost.CloseDialog();
        });

        SukiHost.ShowDialog(new StackPanel()
        {
            DataContext = VisitSelectedItem,
            Children =
            {
                new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Classes = { "h2" },
                    Text = "Удалить?"
                },
                new Button()
                {
                    Content = "Да",
                    Classes = { "Primary" },
                    Command = delete,
                    Foreground = Brushes.White,
                    [!Button.CommandParameterProperty] = new Binding(".")
                },
                new Button()
                {
                    Content = "Закрыть",
                    Command = ReactiveCommand.Create(SukiHost.CloseDialog)
                }
            }
        }, allowBackgroundClose: true);
    }
    
    private AvaloniaList<Employee> GetEmployeesFromDb()
    {
        AvaloniaList<Employee> employees = new AvaloniaList<Employee>();

        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            try
            {
                connection.Open();
                string selectAllEmployees = """
                                         SELECT employee.Id, EmployeeName, RoleName, Role, Login, Password From Employee
                                         join role on employee.role = role.Id
                                         """;
                MySqlCommand cmd = new MySqlCommand(selectAllEmployees, connection);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Employee employeesItem = new Employee();
                    if (!reader.IsDBNull(reader.GetOrdinal("Id")))
                    {
                        employeesItem.Id = reader.GetInt32("Id");
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("EmployeeName")))
                    {
                        employeesItem.EmployeeName = reader.GetString("EmployeeName");
                    }
                    
                    if (!reader.IsDBNull(reader.GetOrdinal("Role")))
                    {
                        employeesItem.Role = reader.GetInt32("Role");
                    }
                    
                    if (!reader.IsDBNull(reader.GetOrdinal("RoleName")))
                    {
                        employeesItem.RoleName = reader.GetString("RoleName");
                    }
                    
                    if (!reader.IsDBNull(reader.GetOrdinal("Login")))
                    {
                        employeesItem.Login = reader.GetString("Login");
                    }
                    
                    if (!reader.IsDBNull(reader.GetOrdinal("Password")))
                    {
                        employeesItem.Password = reader.GetString("Password");
                    }

                    employees.Add(employeesItem);
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Ошибка подключения к БД: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        return employees;
    }
    
    private AvaloniaList<Employee> _employee;

    public AvaloniaList<Employee> Employee
    {
        get => _employee;
        set => this.RaiseAndSetIfChanged(ref _employee, value);
    }
    
    public void OnNew(Employee employee) {
        Employee.Add(employee);
    }
    
    private Employee _employeeSelectedItem;

    public Employee EmployeeSelectedItem {
        get => _employeeSelectedItem;
        set => this.RaiseAndSetIfChanged(ref _employeeSelectedItem, value);
    }
    
    public void OnDelete(Employee employee) {
        Employee.Remove(employee);
    }
    
    public void OnEdit(Employee employee) {
        Employee.Replace(EmployeeSelectedItem, employee);
    }

    public void AddEmployeeToDB()
    {
        var db = new DataBaseAdd();

        var employee = new Employee();

        var roles = new List<Role>();
        {
            using var connection = new MySqlConnection(DataBaseConnectionString.ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("""
                                             select * from Role;
                                             """, connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read() && reader.HasRows)
            {
                var item = new Role()
                {
                    Id = reader.GetInt32("Id"),
                    RoleName = reader.GetString("RoleName")
                };
                roles.Add(item);
            }
        }
        
        var add = ReactiveCommand.Create((Employee i) =>
        {
            var newId = db.InsertData(
                "Employee",
                new MySqlParameter("@EmployeeName", MySqlDbType.VarChar)
                {
                    Value = i.EmployeeName
                },
                new MySqlParameter("@Role", MySqlDbType.Int32)
                {
                    Value = i.Role
                },
                new MySqlParameter("@Login", MySqlDbType.VarChar)
                {
                    Value = i.Login
                },
                new MySqlParameter("@Password", MySqlDbType.VarChar)
                {
                    Value = i.Password
                }
            );
            i.Id = newId;
            i.RoleName = roles.FirstOrDefault(x => x.Id == i.Role).RoleName;
            OnNew(i);
        });

        SukiHost.ShowDialog(new StackPanel()
        {
            DataContext = new Employee(),
            Children =
            {
                new TextBox()
                {
                    Watermark = "Имя сотрудника",
                    [!TextBox.TextProperty] = new Binding("EmployeeName")
                },
                new ComboBox()
                {
                    ItemsSource = roles,
                    Name = "RoleComboBox",
                    DisplayMemberBinding = new Binding("RoleName"),
                    [!ComboBox.SelectedValueProperty] = new Binding("Role"),
                    SelectedValueBinding = new Binding("Id")
                },
                new TextBox()
                {
                    Watermark = "Логин",
                    [!TextBox.TextProperty] = new Binding("Login")
                },
                new TextBox()
                {
                    Watermark = "Пароль",
                    [!TextBox.TextProperty] = new Binding("Password")
                },
                new Button()
                {
                    Content = "Добавить",
                    Classes = { "Primary" },
                    Command = add,
                    Foreground = Brushes.White,
                    [!Button.CommandParameterProperty] = new Binding(".")
                },
                new Button()
                {
                    Content = "Закрыть",
                    Command = ReactiveCommand.Create(SukiHost.CloseDialog)
                }
            }
        }, allowBackgroundClose: true);
    }

    public void EditEmployeeInDB()
    {
        var db = new DataBaseEdit();
        int employeeId = EmployeeSelectedItem.Id;
        var roles = new List<Role>();
        {
            using var connection = new MySqlConnection(DataBaseConnectionString.ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("""
                                             select * from Role;
                                             """, connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read() && reader.HasRows)
            {
                var item = new Role()
                {
                    Id = reader.GetInt32("Id"),
                    RoleName = reader.GetString("RoleName")
                };
                roles.Add(item);
            }
        }
        
        var edit = ReactiveCommand.Create<Employee>((i) =>
        {
            i.RoleName = roles.FirstOrDefault(x => x.Id == i.Role).RoleName;
            db.EditData(
                "Employee",
                employeeId,
                new MySqlParameter("@EmployeeName", MySqlDbType.VarChar)
                {
                    Value = i.EmployeeName
                },
                new MySqlParameter("Role", MySqlDbType.Int32)
                {
                    Value = i.Role
                },
                new MySqlParameter("Login", MySqlDbType.VarChar)
                {
                    Value = i.Login
                },
                new MySqlParameter("Password", MySqlDbType.VarChar)
                {
                    Value = i.Password
                }
            );
            OnEdit(i);
            SukiHost.CloseDialog();
        });

        var dataContext = new Employee()
        {
            Id = EmployeeSelectedItem.Id,
            EmployeeName = EmployeeSelectedItem.EmployeeName,
            Role = EmployeeSelectedItem.Role,
            Login = EmployeeSelectedItem.Login,
            Password = EmployeeSelectedItem.Password
        };
        SukiHost.ShowDialog(new StackPanel()
        {
            DataContext = dataContext,
            Children =
            {
                new TextBox()
                {
                    [!TextBox.TextProperty] = new Binding("EmployeeName")
                },
                new ComboBox()
                {
                    ItemsSource = roles,
                    Name = "RolesComboBox",
                    DisplayMemberBinding = new Binding("RoleName"),
                    [!ComboBox.SelectedValueProperty] = new Binding("Role"),
                    SelectedValueBinding = new Binding("Id")
                },
                new TextBox()
                {
                    [!TextBox.TextProperty] = new Binding("Login")
                },
                new TextBox()
                {
                    [!TextBox.TextProperty] = new Binding("Password")
                },
                new Button()
                {
                    Content = "Обновить",
                    Classes = { "Primary" },
                    Command = edit,
                    Foreground = Brushes.White,
                    [!Button.CommandParameterProperty] = new Binding(".")
                },
                new Button()
                {
                    Content = "Закрыть",
                    Command = ReactiveCommand.Create(SukiHost.CloseDialog)
                }
            }
        }, allowBackgroundClose: true);
    }

    public void DeleteEmployeeFromDB()
    {
        if (EmployeeSelectedItem is null)
        {
            return;
        }
        var db = new DataBaseDelete();
        int employeeId = EmployeeSelectedItem.Id;
        var delete = ReactiveCommand.Create<Employee>((i) =>
        {
            db.DeleteData(
                "Employee",
                employeeId
            );
            OnDelete(i);
            SukiHost.CloseDialog();
        });

        SukiHost.ShowDialog(new StackPanel()
        {
            DataContext = EmployeeSelectedItem,
            Children =
            {
                new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Classes = { "h2" },
                    Text = "Удалить?"
                },
                new Button()
                {
                    Content = "Да",
                    Classes = { "Primary" },
                    Command = delete,
                    Foreground = Brushes.White,
                    [!Button.CommandParameterProperty] = new Binding(".")
                },
                new Button()
                {
                    Content = "Закрыть",
                    Command = ReactiveCommand.Create(SukiHost.CloseDialog)
                }
            }
        }, allowBackgroundClose: true);
    }
    
    public AvaloniaList<Visit> GetFilteredVisits(ReportParameters parameters)
    {
        return new AvaloniaList<Visit>(_visit.Where(visit => visit.Date >= parameters.StartDate && visit.Date <= parameters.EndDate));
    }

    private string _report;

    public string Report
    {
        get => _report;
        set => this.RaiseAndSetIfChanged(ref _report, value);
    }

    private DateTimeOffset _startDate;
    private DateTimeOffset _endDate;

    public DateTimeOffset StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    public DateTimeOffset EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }
    public void ReportGen()
    {
        ReportParameters reportParameters = new ReportParameters();
        reportParameters.StartDate = StartDate;
        reportParameters.EndDate = EndDate;
        GenerateReport(reportParameters);
    }
    
    public void GenerateReport(ReportParameters parameters)
    {
        var filteredVisits = GetFilteredVisits(parameters);
        var textReport = GenerateTextReport(filteredVisits);
        Report = textReport;
    }
    
    public string GenerateTextReport(AvaloniaList<Visit> visits)
    {
        StringBuilder reportBuilder = new StringBuilder();
        reportBuilder.AppendLine("Отчет о посещениях клиентов:");
        reportBuilder.AppendLine("Дата\t           Время\t     Цель\t     Клиент\t               Сотрудник");

        foreach (var visit in visits)
        {
            reportBuilder.AppendLine($"{visit.Date.ToString(string.Format("yyyy-MM-dd"))}\t{visit.Time}\t{visit.Purpose}\t{visit.ClientName}\t{visit.EmployeeName}");
        }

        return reportBuilder.ToString();
    }
}