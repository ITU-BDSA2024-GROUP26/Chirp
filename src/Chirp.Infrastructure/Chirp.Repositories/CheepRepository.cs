namespace Chirp.Repositories;

using Core;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

public class CheepRepository : ICheepRepository
{
    private readonly CheepDBContext _context; 
    public CheepRepository(CheepDBContext context) 
    {
        _context = context;
        DbInitializer.SeedDatabase(_context);
    }
    
    public async Task CreateCheep(Cheep newCheep)
    {
        await _context.AddAsync(newCheep);
        await _context.SaveChangesAsync(); 
    }

    public async Task<ICollection<Cheep>> ReadCheeps(int limit = -1, int offset = 0, string? authorNameRegex = null)
    {
        IQueryable<Cheep>? query = 
                    (from cheep in _context.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                    where Regex.IsMatch(cheep.Author.Name, authorNameRegex)
                    select cheep).Skip(offset);
        
        if(limit > 0) {
            query = query.Take(limit);
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