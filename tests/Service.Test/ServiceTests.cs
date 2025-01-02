using Core;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Service.Test;

/// <summary>
/// Integration tests for our service class
/// </summary>
public class ServiceTests
{
    private static async Task<(ICheepService, CheepDbContext, ICheepRepository, IAuthorRepository, INotificationRepository, UserManager<Author>)> GetContext() // creates a seperate database for every test
    {
        DbContextOptions<CheepDbContext> _options = new DbContextOptionsBuilder<CheepDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).EnableSensitiveDataLogging()
        .Options;

        var _context = new CheepDbContext(_options);
        var store = new UserStore<Author>(_context);
        var userManager = new UserManager<Author>(store, null, new PasswordHasher<Author>(),
            null, null, null, null, null, null);
        

        await DbInitializer.SeedDatabase(_context, userManager);
        
        var _notificationRepository = new NotificationRepository(_context);
        var _cheepRepository = new CheepRepository(_context, _notificationRepository);
        var _authorRepository = new AuthorRepository(_context);
        var _dbRepository = new DbRepository(_context, userManager);
        var _service = new CheepService(_cheepRepository, _authorRepository, _dbRepository, _notificationRepository);

        return(_service, _context, _cheepRepository, _authorRepository, _notificationRepository, userManager);
    }
    
    private async Task Dispose(CheepDbContext _context)
    {
        await _context!.DisposeAsync();
    }

    private async Task<Author?> Register(string username, IAuthorRepository _authorRepository, UserManager<Author> _userManager)
    {
        Author author = new() { UserName = username, Email = username + "@test.com", Cheeps = new List<Cheep>()};
        author.NormalizedUserName = author.UserName;
        author.NormalizedEmail = author.Email;
        await _userManager!.CreateAsync(author);
        return await _authorRepository!.FindAuthorByName(username);
    }

    [Fact] 
    public async Task TestNoSelfFollowInCircularFollow()
    {
        ICheepService _service; 
        IAuthorRepository _authorRepository; 
        CheepDbContext _context; 
        (_service, _context, _, _authorRepository,_, _) = await GetContext();

        await _authorRepository!.AddOrRemoveFollower("Helge", "Adrian");
        await _authorRepository!.AddOrRemoveFollower("Adrian", "Helge");

        var followingList = await _service!.GetFollowingAuthorsAsync("Helge"); 

        Assert.Contains(true ,followingList.Select(a => a.UserName == "Adrian")); 
        Assert.DoesNotContain(true, followingList.Select(a => a.UserName == "Helge")); 

        await Dispose(_context);
    }

    [Fact]
    public async Task AboutMe_NoFollowingNoCheeps_DownloadedContentIsCorrect()
    {
        UserManager<Author> _userManager; 
        IAuthorRepository _authorRepository;
        CheepDbContext _context; 
        ICheepService _service;
        (_service, _context, _, _authorRepository, _, _userManager) = await GetContext(); 

        var author = await Register("test", _authorRepository, _userManager);
        var (fileBytes, _, _) = await _service!.DownloadAuthorInfo(author!.UserName!, author!.Email!);
        var content = Encoding.UTF8.GetString(fileBytes);
        const string expectedContent = """
                                       test's information:
                                       -----------------------
                                       Name: test
                                       Email: test@test.com
                                       Following:
                                       - No following
                                       Cheeps:
                                       - No Cheeps posted yet

                                       """;
        Assert.Equal(expectedContent, content);

        await Dispose(_context);
    }

    [Fact]
    public async Task AboutMe_Following_DownloadedContentIsCorrect()
    {
        UserManager<Author> _userManager; 
        IAuthorRepository _authorRepository;
        CheepDbContext _context; 
        ICheepService _service;
        (_service, _context, _, _authorRepository, _, _userManager) = await GetContext(); 


        var author = await Register("test", _authorRepository, _userManager);
        await _authorRepository!.AddOrRemoveFollower("test", "Helge");
        var (fileBytes, _, _) = await _service!.DownloadAuthorInfo(author!.UserName!, author!.Email!);
        var content = Encoding.UTF8.GetString(fileBytes);
        const string expectedContent = """
                                       test's information:
                                       -----------------------
                                       Name: test
                                       Email: test@test.com
                                       Following:
                                       - Helge
                                       Cheeps:
                                       - No Cheeps posted yet

                                       """;
        Assert.Equal(expectedContent, content);

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
        (_service, _context, _cheepRepository, _authorRepository, _, _) = await GetContext();
        if(follow) {await _authorRepository!.AddOrRemoveFollower("Helge", "Adrian");}

        var adrian = await _authorRepository.FindAuthorByName("Adrian"); 
        var helge = await _authorRepository.FindAuthorByName("Helge");

        await _cheepRepository!.CreateCheep(new Cheep {
            Author = adrian, Text=cheepContent
        }); 

        var notifs = await _service!.GetAuthorsNotifications("Helge", getOld); 
        
        
        Assert.True(notifs.All(n => {
            return n.AuthorName == adrian!.UserName && n.CheepContent == cheepContent && n.TagNotification;  
        }));
        await Dispose(_context);
    }
}