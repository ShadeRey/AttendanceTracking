using System;
using System.Linq;
using MySqlConnector;

namespace AttendanceTracking.DataBase;

public class DataBaseAdd
{
    public static readonly string ConnectionString = DataBaseConnectionString.ConnectionString;

    public int InsertData(string tableName, params MySqlParameter[] parameters)
    {
        using MySqlConnection connection = new MySqlConnection(ConnectionString);
        try
        {
            connection.Open();
            using MySqlCommand command = connection.CreateCommand();
            var paramString = string.Join(',', parameters.Select(x => x.ParameterName));
            var columnsString = string.Join(',', parameters.Select(x => x.ParameterName.Replace("@", "")));
            command.CommandText =
                $"INSERT INTO {tableName}({columnsString}) VALUES ({paramString}); SELECT LAST_INSERT_ID();";
            command.Parameters.AddRange(parameters);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        catch (Exception e)
        {
            Console.WriteLine("Ошибка добавления: " + e.Message);
            throw;
        }
        finally
        {
            connection.Close();
        }
    }
}