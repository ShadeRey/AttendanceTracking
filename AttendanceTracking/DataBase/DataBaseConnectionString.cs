namespace AttendanceTracking.DataBase;

public class DataBaseConnectionString
{
    private static string _connectionString //= "server=10.10.1.24;user=user_01;password=user01pro;database=pro1_23;";
                                            = "Server=localhost;Database=UP;User Id=root;Password=sharaga228;";

    public static string ConnectionString
    {
        get => _connectionString;
    }
}