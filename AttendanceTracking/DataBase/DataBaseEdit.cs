using System;
using System.Linq;
using MySqlConnector;

namespace AttendanceTracking.DataBase;

public class DataBaseEdit
{
    public static readonly string ConnectionString = DataBaseConnectionString.ConnectionString;

    public void EditData(string tableName, int id, params MySqlParameter[] parameters)
    {
        using MySqlConnection connection = new MySqlConnection(ConnectionString);
        try
        {
            connection.Open();
            using MySqlCommand command = connection.CreateCommand();
            var paramString = string.Join(',',
                parameters.Select(x => $"{x.ParameterName.Replace("@", "")} = {x.ParameterName}"));
            command.CommandText = $"UPDATE {tableName} SET {paramString} WHERE ID = @Id;";
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddRange(parameters);
            command.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Console.WriteLine("Ошибка редактирования: " + e.Message);
        }
        finally
        {
            connection.Close();
        }
    }
}