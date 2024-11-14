using Chirp.Core.Entities;

namespace Chirp.Infrastructure.Repositories;

using Core;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

public class CheepRepository(CheepDbContext context) : ICheepRepository
{
    public async Task CreateCheep(Cheep newCheep)
    {
        await context.AddAsync(newCheep);
        await context.SaveChangesAsync(); 
    }
  
    public async Task<Author?> FindAuthorByName(string name)
    {
        return await context.Users.FirstOrDefaultAsync(a=> a.Name == name);
    } 
    public async Task<Author?> FindAuthorByEmail(string email)
    {
        return await context.Users.FirstOrDefaultAsync(a=> a.Email == email);
    } 


    public async Task<ICollection<Cheep>> ReadCheeps(int limit = -1, int offset = 0, string? authorNameRegex = null)
    {
        IQueryable<Cheep>? query;
            
        if(authorNameRegex == null) 
        {
            query=  (from cheep in context.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                     orderby cheep.Id descending
                     select cheep)
                    .Skip(offset);
        } else {
            Console.WriteLine("qwe: ", authorNameRegex);
            query =  (from cheep in context.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                     where cheep.Author!.Name! == authorNameRegex!
                     orderby cheep.Id descending
                     select cheep)
                    .Skip(offset);
        } 

        if (limit > 0)
        {
            query = query.Take(limit);
        }

        await context.SaveChangesAsync();

        return await query.ToListAsync();
    }

    public async Task UpdateCheep(int id, string newMessage)
    {
        var query =
                    from cheep in context.Cheeps
                    where cheep.Id == id
                    select cheep;

        // Possible todo: record the update timestamp
        // https://stackoverflow.com/questions/20832684/update-records-using-linq 
        await query.ForEachAsync(cheep => cheep.Text = newMessage);

        await context.SaveChangesAsync();
    }

}