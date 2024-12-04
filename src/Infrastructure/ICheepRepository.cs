using Core;

namespace Infrastructure;

using Core;

public interface ICheepRepository 
{
    public Task CreateCheep(Cheep newCheep); 
    public Task<ICollection<Cheep>> ReadCheeps(int limit, int offset, string? authorNameRegex=null); 
    // given a cheep ID this method updates the 
    public Task UpdateCheep(int id, string newMessage); 
    public Task<ICollection<Cheep>> GetPrivateTimelineCheeps(string userName, int limit, int offset);
    public int GetTotalCheepsAccount(string? author = null);
}