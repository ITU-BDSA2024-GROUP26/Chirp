
using System.Data;
using System.Text.RegularExpressions;
using Chirp.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

public interface IMessageRepository 
{
    public Task CreateCheep(CheepDTO newCheep); 
    public Task<ICollection<CheepDTO>> ReadCheeps(string? authorNameRegex, int limit, int offset); 

    // given a cheep ID this method updates the 
    public Task UpdateCheep(int id, string newMessage); 
}

public class MessageRepository : IMessageRepository
{
    private readonly CheepDBContext _context; 
    public MessageRepository(CheepDBContext context) 
    {
        _context = context;
    }

    // The timestamp of newCheep is ignored, it can be null or empty just fine
    public async Task CreateCheep(CheepDTO newCheep)
    {
        // since we can expect that there will only be a few milliseconds between the time when the user clicks submit and this method is called
        // we just set the timestamp to now
        var result = await _context.AddAsync(new Cheep(newCheep.AuthorName, newCheep.MessageContent, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

        await _context.SaveChangesAsync(); 

    }

    public async Task<ICollection<CheepDTO>> ReadCheeps(string? authorNameRegex, int limit = -1, int offset = 0)
    {
        authorNameRegex ??= ".*"; 
        // readCheeps(A*) readCheeps(*+ B*+)
        IQueryable<CheepDTO>? query = 
                    (from cheep in _context.Cheeps 
                    where Regex.IsMatch(cheep.Author.Name, authorNameRegex)
                    select new CheepDTO(cheep)).Skip(offset);
        if(limit > 0) {
            query.Take(limit);
        }
        

        await _context.SaveChangesAsync();

        return await query.ToListAsync();
    }

    public async Task UpdateCheep(int id, string newMessage)
    {
        var query = 
                    from cheep in _context.Cheeps 
                    where cheep.CheepId == id 
                    select cheep; 
        
        // Possible todo: record the update timestamp
        // https://stackoverflow.com/questions/20832684/update-records-using-linq 
        await query.ForEachAsync(cheep => cheep.Text = newMessage); 

        await _context.SaveChangesAsync();
    }
}