using Core;

namespace Infrastructure;


public interface IAuthorRepository 
{
    public Task<Author?>FindAuthorByName(string name); 
    public Task<Author?>FindAuthorByEmail(string email); 
    public Task<ICollection<Author>> GetAuthorsFollowing(string name); 
    public Task AddOrRemoveFollower(string userName, string usernmToFollow);
    public Task<Author?>DeleteAuthorByName(string name); 
}