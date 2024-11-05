using Chirp.Core.Entities;

namespace Chirp.Infrastructure.Repositories;

using Core;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

public class CheepRepository : ICheepRepository
{
    private readonly CheepDBContext _context; 
    public CheepRepository(CheepDBContext context) 
    {
        _context = context;
        
        if(CheepDBContext.testingSetup) {
            _context.Database.Migrate();
        }

        DbInitializer.SeedDatabase(_context);
    }
    
    public async Task CreateCheep(Cheep newCheep)
    {
        //  check for author exists with FindAuthorbyName
        // if author doesn't exists, call CreateAuthor. 
        // else assign the author that exists to author of the new cheep
        var authorExists = await FindAuthorbyName(newCheep.Author.Name);
        if (authorExists == null) 
        {
                await CreateAuthor(newCheep.Author); 
        }
        else 
        {
            newCheep.Author = authorExists; 
        }

        await _context.AddAsync(newCheep);
        await _context.SaveChangesAsync(); 

    }

    public async Task CreateAuthor(Author newAuthor)
    {

        await _context.AddAsync(newAuthor);
        await _context.SaveChangesAsync(); 

    }
  
    public async Task<Author?> FindAuthorbyName(string name)
    {

        var author = await _context.Authors.FirstOrDefaultAsync(a=> a.Name == name);
        return author;  

    } 
    public async Task<Author?> FindAuthorbyEmail(string email)
    {

        var author = await _context.Authors.FirstOrDefaultAsync(a=> a.Email == email);
        return author;  

    } 


    public async Task<ICollection<Cheep>> ReadCheeps(int limit = -1, int offset = 0, string? authorNameRegex = null)
    {
        IQueryable<Cheep>? query =
                    (from cheep in _context.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                     where Regex.IsMatch(cheep.Author!.Name!, authorNameRegex!)
                     select cheep).Skip(offset);

        if (limit > 0)
        {
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