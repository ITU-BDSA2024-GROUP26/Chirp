using System.Text;
using Core;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq; 

namespace Service.Test;

public class ServiceTests: IAsyncLifetime
{
    private ICheepRepository? _cheepRepository;
    private IAuthorRepository? _authorRepository; 
    private IDbRepository? _dbRepository;
    private ICheepService? _service; 
    private CheepDbContext? _context;
    private UserManager<Author>? _userManager;
    
    private readonly DbContextOptions<CheepDbContext> _options = new DbContextOptionsBuilder<CheepDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    public async Task InitializeAsync()
    {
        _context = new CheepDbContext(_options);
        var store = new UserStore<Author>(_context);
        _userManager = new UserManager<Author>(store, null, new PasswordHasher<Author>(),
            null, null, null, null, null, null);
        _dbRepository = new DbRepository(_context, _userManager);
        
        _cheepRepository = new CheepRepository(_context);
        _authorRepository = new AuthorRepository(_context);
        _service = new CheepService(_cheepRepository, _authorRepository, _dbRepository);
        await _service.SeedDatabaseAsync();
    }

    public async Task DisposeAsync() 
    {
        await _context!.DisposeAsync();
    }

    private async Task<Author?> Register(string username)
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
        await _authorRepository!.AddOrRemoveFollower("Helge", "Adrian");
        await _authorRepository!.AddOrRemoveFollower("Adrian", "Helge");

        var followingList = await _service!.GetFollowingAuthorsAsync("Helge"); 

        Assert.Contains(true ,followingList.Select(a => a.UserName == "Adrian")); 
        Assert.DoesNotContain(true, followingList.Select(a => a.UserName == "Helge")); 
    }

    [Fact]
    public async Task AboutMe_Download_FileIsCorrect()
    {
        var helge = await _authorRepository!.FindAuthorByName("Helge");
        var (fileBytes, _, _) = await _service!.DownloadAuthorInfo(helge!);
        var content = Encoding.UTF8.GetString(fileBytes);
        const string expectedContent = """
                                       Helge's information:
                                       -----------------------
                                       Name: Helge
                                       Email: ropf@itu.dk
                                       Following:
                                       - No following
                                       Cheeps:
                                       - "Hello, BDSA students!" (01/08/2023 12.16.48)
                                       
                                       """;
        Assert.Equal(expectedContent, content);
    }
    
    [Fact]
    public async Task AboutMe_NoFollowingNoCheeps_DownloadedContentIsCorrect()
    {
        var author = await Register("test");
        var (fileBytes, _, _) = await _service!.DownloadAuthorInfo(author!);
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
    }
    
    [Fact]
    public async Task AboutMe_Following_DownloadedContentIsCorrect()
    {
        var author = await Register("test");
        await _authorRepository!.AddOrRemoveFollower("test", "Helge");
        var (fileBytes, _, _) = await _service!.DownloadAuthorInfo(author!);
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
    }
    
    [Fact]
    public async Task AboutMe_Cheep_DownloadedContentIsCorrect()
    {
        var author = await Register("test");
        Cheep cheep = new() { Author = author, Text = "test message" };
        await _cheepRepository!.CreateCheep(cheep);
        var (fileBytes, _, _) = await _service!.DownloadAuthorInfo(author!);
        var content = Encoding.UTF8.GetString(fileBytes);
        const string expectedContent = """
                                       test's information:
                                       -----------------------
                                       Name: test
                                       Email: test@test.com
                                       Following:
                                       - No following
                                       Cheeps:
                                       - "test message" (01/01/0001 00.00.00)
                                       
                                       """;
        Assert.Equal(expectedContent, content);
    }
}