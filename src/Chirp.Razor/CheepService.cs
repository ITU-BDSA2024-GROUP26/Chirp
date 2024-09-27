using Chirp.SQLite;

public record CheepViewModel(string username, string text, string pub_date)
{
    public CheepViewModel() : this("defaultUsername", "defaultText", "defaultPubDate") { }
}

public interface ICheepService
{
    public IEnumerable<CheepViewModel> GetCheeps(int page);
    public IEnumerable<CheepViewModel> GetCheepsFromAuthor(string author, int page);
}

public class CheepService : ICheepService
{
    private readonly ISQLDatabase _database;
    private readonly int _maxCheeps = 32;
    
    private CheepService()
    {
        _database = DBFacade.Instance;
    }
    
    public void SetDBPath(string path)
    {
        DBFacade.SetDBPath(path);
    }
    
    public IEnumerable<CheepViewModel> GetCheeps(int page)
    {
        var indices = getIndices(page);
        
        var query = $"""
                        SELECT user.username, message.text, message.pub_date 
                        FROM user
                        JOIN message ON user.user_id = message.author_id
                        WHERE message.message_id BETWEEN {indices[0]} AND {indices[1]}
                    """;

        return _database.ObjectQuery<CheepViewModel>(query);
    }

    public IEnumerable<CheepViewModel> GetCheepsFromAuthor(string author, int page)
    {
        var indices = getIndices(page);
        
        var query = $"""
                         SELECT user.username, message.text, message.pub_date 
                         FROM user
                         JOIN message ON user.user_id = message.author_id
                         WHERE user.username = '{author}'
                         LIMIT {_maxCheeps} OFFSET {indices[0]}
                     """;
        
        return _database.ObjectQuery<CheepViewModel>(query);
    }
    
    private int[] getIndices(int page)
    {
        var startIndex = (page - 1) * _maxCheeps;
        var endIndex = startIndex + _maxCheeps;
        return new int[] { startIndex, endIndex };
    }

    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }

}
