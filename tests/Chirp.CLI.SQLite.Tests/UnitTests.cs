using Chirp.SQLite;

namespace Chirp.CLI.SQLite.Tests;

public class UnitTests : IDisposable
{
    private const string DBPath = "../../../testdb.db";
    private readonly ISQLDatabase _database;

    public UnitTests()
    {
        DBFacade.SetDBPath(DBPath);
        _database = DBFacade.Instance;
        var schema = File.ReadAllText("../../../../../src/Chirp.Razor/schema.sql");
        var dump = File.ReadAllText("../../../dump.sql");
        _database.Execute(schema);
        _database.Execute(dump);
    }

    public void Dispose()
    {
        File.Delete(DBPath);
    }


    [Fact]
    public void TestObjectQuery()
    {
        const string query = "SELECT * FROM user";
        var users = _database.ObjectQuery<UserViewModel>(query);
        Assert.Equal(2, users.Count());
    }
    
    [Fact]
    public void TestFieldQuery()
    {
        const string query = "SELECT username FROM user";
        var users = (List<string>)_database.FieldQuery<string>(query);
        Assert.Equal("Helge", users[0] );
        Assert.Equal("Adrian", users[1] );
    }
}