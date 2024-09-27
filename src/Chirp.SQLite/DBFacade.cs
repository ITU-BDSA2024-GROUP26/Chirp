// TODO: Refactor into a thread-safe singleton

namespace Chirp.SQLite;

using Microsoft.Data.Sqlite;

public sealed class DBFacade : ISQLDatabase
{
    private static DBFacade? _instance;
    private static string? _dataSourcePath;

    private DBFacade()
    {
        SQLitePCL.Batteries.Init();
    }

    public static DBFacade Instance
    {
        get
        {
            if (_dataSourcePath == null)
            {
                throw new System.Exception($"Database path not set. Use the SetPath() static method first.");
            }
            // If _instance is null, create a new instance of DBFacade
            return _instance ??= new DBFacade();
        }
    }
    
    public static void SetDBPath(string path)
    {
        DBFacade._dataSourcePath = $"Data Source={path}";
    }
    
    // where T: new() ensures that T has a constructor and the constructor is parameterless.
    public IEnumerable<T> ObjectQuery<T>(string query) where T: new()
    {
        using SqliteConnection connection = new(_dataSourcePath);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = query;
        var results = new List<T>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            T result = new();
            foreach (var property in typeof(T).GetProperties())
            {
                var value = reader[property.Name];
                if (value is DBNull)
                {
                    throw new System.Exception($"Database column {property.Name} is null.");
                }
                property.SetValue(result, value);
            }
            results.Add(result);
        }

        return results;
    }
    
    public IEnumerable<T> FieldQuery<T>(string query)
    {
        using SqliteConnection connection = new(_dataSourcePath);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = query;
        var results = new List<T>();
        using var reader = command.ExecuteReader();
        if (reader.FieldCount != 1)
        {
            throw new System.Exception("Query must return a single field.");
        }
        while (reader.Read())
        {
            results.Add((T)reader[0]);
        }
        return results;
    }
    
    public void Execute(string commandText)
    {
        using SqliteConnection connection = new(_dataSourcePath);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.ExecuteNonQuery();
    }
}