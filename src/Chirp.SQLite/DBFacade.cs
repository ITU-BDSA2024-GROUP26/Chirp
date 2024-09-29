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
        // Taken from Helge's example
        // Added Pooling=False because if not, something isn't closed properly and temp databases can't be deleted when unit testing.
        DBFacade._dataSourcePath = $"Data Source={path};Pooling=False";
    }
    
    // where T: new() ensures that T has a constructor and the constructor is parameterless.
    // Inspired from Helge's example
    public IEnumerable<T> ObjectQuery<T>(string query) where T: new()
    {
        using SqliteConnection connection = new(_dataSourcePath);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = query;
        var results = new List<T>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) // ensures that we read every row in the table
        {
            T result = new(); // initialize an "empty" instance of T, which we will fill in with the data from the query
            
            // Here we iterate over all the properties of T and set the value of the property to the value of the corresponding column in the database
            foreach (var property in typeof(T).GetProperties())
            {
                var value = reader[property.Name];
                
                // TODO: Don't check this on every row, but once before the loop
                if (value is DBNull) // we need to ensure that the columns correspond to the properties of T
                {
                    throw new System.Exception($"Database column {property.Name} is null.");
                }
                
                // this just sets the property field of result to the value. 
                // example: T is Cheep, property is author and value is "Helge" 
                // then result.author = "Helge" after this line
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
        using var command = connection.CreateCommand();
        command.CommandText = query;
        var results = new List<T>();
        using var reader = command.ExecuteReader();
        if (reader.FieldCount != 1)
        {
            throw new System.Exception("Query must return a single field.");
        }
        while (reader.Read())
        {
            // reader represents a row with the indexes being columns. 
            // We know that we only have a single column which is the data we wish to retrieve, thus we just cast the value of this column to T 
            results.Add((T)reader[0]);
        }
        return results;
    }
    
    // Inspired from Helge's example
    public void Execute(string commandText)
    {
        using SqliteConnection connection = new(_dataSourcePath);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.ExecuteNonQuery(); // ExecuteNonQuery is used for commands that do not return data. (insertion, deletion, etc...)
    }
}