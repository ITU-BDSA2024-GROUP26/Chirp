using Core;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq; 

namespace Service.Test;

public class UnitTest1: IAsyncLifetime
{
    private ICheepRepository? _cheepRepository;
    private IAuthorRepository? _authorRepository; 
    private ICheepService? _service; 
    private CheepDbContext? _context;
    
    private readonly DbContextOptions<CheepDbContext> _options = new DbContextOptionsBuilder<CheepDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    public async Task InitializeAsync()
    {
        _context = new CheepDbContext(_options);
        var store = new UserStore<Author>(_context);
        var userManager = new UserManager<Author>(store, null, new PasswordHasher<Author>(),
            null, null, null, null, null, null);
        await DbInitializer.SeedDatabase(_context, userManager);
        
        _cheepRepository = new Infrastructure.CheepRepository(_context);
        _authorRepository = new Infrastructure.AuthorRepository(_context);
        _service = new CheepService(_cheepRepository, _authorRepository);
    }

    public async Task DisposeAsync() 
    {
        await _context!.DisposeAsync();
    }

    [Fact] 
    public async Task TestNoSelfFollowInCircularFollow()
    {
        await _authorRepository!.AddOrRemoveFollower("Helge", "Adrian");
        await _authorRepository!.AddOrRemoveFollower("Adrian", "Helge");

        var followingList = await _service!.GetFollowingAuthorsAsync("Helge"); 

        Assert.Contains(true ,followingList.Select(a => a.UserName == "Adrian")); 
        Assert.DoesNotContain(true, followingList.Select(a => a.UserName == "Helge")); 
    }

    
}