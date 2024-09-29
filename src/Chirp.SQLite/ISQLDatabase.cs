namespace Chirp.SQLite;

public interface ISQLDatabase
{
    public IEnumerable<T> ObjectQuery<T>(string query) where T: new();
    public IEnumerable<T> FieldQuery<T>(string query);
    public void Execute(string commandText);
}