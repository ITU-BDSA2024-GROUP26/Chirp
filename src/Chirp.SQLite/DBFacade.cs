// TODO: Refactor into a thread-safe singleton

namespace Chirp.SQLite;

using Microsoft.Data.Sqlite;

public sealed class DBFacade
{
    private static DBFacade? _instance;
    private static string? _connectionSt ring;

    private DBFacade() { }

    public static DBFacade Instance
    {
        get
        {
            if (_connectionString == null)
            {
                throw new System.Exception($"Database path not set. Use the SetPath() static method first.");
            }
            return _instance ??= new DBFacade();
        }
    }
    
    public static void SetDBPath(string path)
    {
        DBFacade._connectionString = $"Data Source={path}";
    }
    
    public List<T> Query<T>(string query)
    {
        using SqliteConnection connection = new(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = query;
        var results = new List<T>();
        using var reader = command.ExecuteReader();

        return "";
    }
    
    public void Execute(string commandText)
    {
        using SqliteConnection connection = new(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.ExecuteNonQuery();
    }
}