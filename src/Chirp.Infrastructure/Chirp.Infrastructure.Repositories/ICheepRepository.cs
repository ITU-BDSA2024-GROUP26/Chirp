using Chirp.Core.Entities;

namespace Chirp.Infrastructure.Repositories;

using Chirp.Core;

public interface ICheepRepository 
{
    public Task CreateCheep(Cheep newCheep); 
    public Task<Author?>FindAuthorbyName(string name); 
    public Task<Author?>FindAuthorbyEmail(string email); 
    public Task<ICollection<Cheep>> ReadCheeps(int limit, int offset, string? authorNameRegex); 

    // given a cheep ID this method updates the 
    public Task UpdateCheep(int id, string newMessage); 

    
    



}