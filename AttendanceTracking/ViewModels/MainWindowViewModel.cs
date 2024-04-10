using System;
using AttendanceTracking.DataBase;
using AttendanceTracking.Models;
using AttendanceTracking.Views;
using MySqlConnector;
using ReactiveUI;

namespace AttendanceTracking.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public AdministratorViewModel AdministratorViewModel { get; set; } = new AdministratorViewModel();
    public EmployeeViewModel EmployeeViewModel { get; set; } = new EmployeeViewModel();
    private string _login;
    private string _password;
    private bool _invalidVisible;
    private bool _loginDialogOpen = true;
    private bool _captchaVisible;
    private int _loginDialogHeight = 250;
    private string? _captchaText = null;
    private string _captchaTextBoxText = String.Empty;

    public string Login
    {
        get => _login;
        set => this.RaiseAndSetIfChanged(ref _login, value);
    }

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public bool InvalidVisible
    {
        get => _invalidVisible;
        set => this.RaiseAndSetIfChanged(ref _invalidVisible, value);
    }

    public bool LoginDialogOpen
    {
        get => _loginDialogOpen;
        set => this.RaiseAndSetIfChanged(ref _loginDialogOpen, value);
    }

    public bool CaptchaVisible
    {
        get => _captchaVisible;
        set
        {
            this.RaisePropertyChanging();
            _captchaVisible = value;
            this.RaisePropertyChanged();
        }
    }

    public int LoginDialogHeight
    {
        get => _loginDialogHeight;
        set => this.RaiseAndSetIfChanged(ref _loginDialogHeight, value);
    }

    public string? CaptchaText
    {
        get => _captchaText;
        set => this.RaiseAndSetIfChanged(ref _captchaText, value);
    }

    public string CaptchaTextBoxText
    {
        get => _captchaTextBoxText;
        set => this.RaiseAndSetIfChanged(ref _captchaTextBoxText, value);
    }
    
    private User? ValidateUser(string login, string password)
    {
        string connectionString = DataBaseConnectionString.ConnectionString;

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT Role FROM Employee WHERE Login = @Login AND Password = @Password";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Login", login);
                command.Parameters.AddWithValue("@Password", password);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read()) {
                        return new User() {
                            Role = reader["Role"].ToString(),
                        };
                    }
                }
            }
        }

        return null;
    }
    
    private string GetEmployeeName(string login, string password)
    {
        string connectionString = DataBaseConnectionString.ConnectionString;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT EmployeeName FROM Employee WHERE Login = @Login AND Password = @Password";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Login", login);
                command.Parameters.AddWithValue("@Password", password);

                object result = command.ExecuteScalar();
                if (result != null)
                {
                    return result.ToString();
                }
            }
        }

        return null;
    }
    
    public void LoginButton()
    {
        string login = Login;
        string password = Password;
        User role = ValidateUser(login, password);
        AdministratorViewModel.FullName = GetEmployeeName(login, password);
        if (role != null)
        {
            if (!CaptchaVisible || CaptchaTextBoxText == CaptchaText)
            {
                switch (role.Role)
                {
                    case "1":
                        AdministratorViewModel.AdministratorViewVisible = true;
                        LoginDialogOpen = false;
                        break;
                    case "2":
                        EmployeeViewModel.EmployeeViewVisible = true;
                        LoginDialogOpen = false;
                        break;
                }
            }
            else
            {
                CaptchaVisible = false;
                CaptchaVisible = true;
            }
        }
        else
        {
            InvalidVisible = true;
            CaptchaVisible = false;
            CaptchaVisible = true;
            LoginDialogHeight = 380;
        }
    }
}