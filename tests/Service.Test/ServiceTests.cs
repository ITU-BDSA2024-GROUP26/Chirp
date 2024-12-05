using Core;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq; 

namespace Service.Test;

public class ServiceTests
{
    private static async Task<(ICheepService, CheepDbContext, ICheepRepository, IAuthorRepository)> GetContext() // creates a seperate database for every test
    {
        DbContextOptions<CheepDbContext> _options = new DbContextOptionsBuilder<CheepDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).EnableSensitiveDataLogging()
        .Options;

        var _context = new CheepDbContext(_options);
        var store = new UserStore<Author>(_context);
        var userManager = new UserManager<Author>(store, null, new PasswordHasher<Author>(),
            null, null, null, null, null, null);
        

        await DbInitializer.SeedDatabase(_context, userManager);
        
        var _cheepRepository = new CheepRepository(_context);
        var _authorRepository = new AuthorRepository(_context);
        var _dbRepository = new DbRepository(_context, userManager);
        var _service = new CheepService(_cheepRepository, _authorRepository, _dbRepository);

        return(_service, _context, _cheepRepository, _authorRepository);
    }
    
    private async Task Dispose(CheepDbContext _context)
    {
        await _context!.DisposeAsync();
    }

    [Fact] 
    public async Task TestNoSelfFollowInCircularFollow()
    {
        ICheepService _service; 
        IAuthorRepository _authorRepository; 
        CheepDbContext _context; 
        (_service, _context, _, _authorRepository) = await GetContext();

        await _authorRepository!.AddOrRemoveFollower("Helge", "Adrian");
        await _authorRepository!.AddOrRemoveFollower("Adrian", "Helge");

        var followingList = await _service!.GetFollowingAuthorsAsync("Helge"); 

        Assert.Contains(true ,followingList.Select(a => a.UserName == "Adrian")); 
        Assert.DoesNotContain(true, followingList.Select(a => a.UserName == "Helge")); 

        await Dispose(_context);
    }

    [Theory]  // note, every test contains a tag, but we have coverage that this doesn't matter from repository tests
    [InlineData("Test123 @Helge", true, true)]
    [InlineData("@Helge test123", true, true)]
    [InlineData("qwe @Helge test", true, true)]
    [InlineData("@Helge", true, true)]
    [InlineData(" @Helge ", true, true)]
    [InlineData("Test123 @Helge", false, true)]
    [InlineData("@Helge test123", false, true)]
    [InlineData("qwe @Helge test", false, true)]
    [InlineData("@Helge", false, true)]
    [InlineData(" @Helge ", false, true)]
    [InlineData("Test123 @Helge",  false, false)]
    [InlineData("@Helge test123",  false, false)]
    [InlineData("qwe @Helge test",  false, false)]
    [InlineData("@Helge",  false, false)]
    [InlineData(" @Helge ",  false, false)]
    [InlineData("Test123 @Helge",  true, false)]
    [InlineData("@Helge test123",  true, false)]
    [InlineData("qwe @Helge test",  true, false)]
    [InlineData("@Helge",  true, false)]
    [InlineData(" @Helge ",  true, false)]
    public async Task TestNotificationsConversionToDTO(string cheepContent, bool getOld, bool follow)
    {
        ICheepService _service; 
        IAuthorRepository _authorRepository; 
        ICheepRepository _cheepRepository; 
        CheepDbContext _context; 
        (_service, _context, _cheepRepository, _authorRepository) = await GetContext();
        if(follow) {await _authorRepository!.AddOrRemoveFollower("Helge", "Adrian");}

        var adrian = await _authorRepository.FindAuthorByName("Adrian"); 
        var helge = await _authorRepository.FindAuthorByName("Helge");

        await _cheepRepository!.CreateCheep(new Cheep {
            Author = adrian, Text=cheepContent
        }); 

        var notifs = await _service!.GetAuthorsNotifications("Helge", getOld); 
        
        
        Assert.True(notifs.All(n => {
            return n.authorName == adrian!.UserName && n.cheepContent == cheepContent && n.tagNotification;  
        }));
        await Dispose(_context);
    }


}