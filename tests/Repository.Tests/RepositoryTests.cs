using Core;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;
using Xunit.Abstractions;


namespace Repository.Tests;
public class RepositoryTests
{
    private static async Task<( CheepDbContext, ICheepRepository, IAuthorRepository, INotificationRepository)> GetContext() // creates a seperate database for every test
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

        return(_context, _cheepRepository, _authorRepository, _notificationRepository);
    }
    
    private async Task Dispose(CheepDbContext _context)
    {
        await _context!.DisposeAsync();
    }


    [Fact]
    public async Task CreateCheep_AddsCheepToDatabase()
    {
		ICheepRepository _cheepRepository; 
        CheepDbContext _context; 
        (_context, _cheepRepository, _, _) = await GetContext();
        // Arrange
        const string text = "Hello world";
        var newCheep = new Cheep
        {
            Text = text,
            Author = _context!.Users.First() // Assuming Authors are seeded
        };
    
        // Act
        await _cheepRepository!.CreateCheep(newCheep);
    
        // Assert
        var cheeps = await _context!.Cheeps.ToListAsync();
        Assert.Equal(text, cheeps.Last().Text);

        await Dispose(_context);
    }

    [Fact]
    public async Task ReadCheeps_ReturnsAllCheeps()
    {
		ICheepRepository _cheepRepository; 
        CheepDbContext _context; 
        (_context, _cheepRepository, _, _) = await GetContext();
        var expectedNumCheeps = _context!.Cheeps.Count();
        var cheeps = await _cheepRepository!.ReadCheeps(-1, 0);
        var actualNumCheeps = cheeps.Count;
    
        // Assert
        Assert.Equal(expectedNumCheeps, actualNumCheeps);

        await Dispose(_context);
    }


    [Fact]
    public async Task ReadCheeps_ReturnsLimitedCheeps()
    {
		ICheepRepository _cheepRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _, _) = await GetContext();
        // Arrange
        int limit = 2;

        // Act
        var cheeps = await _cheepRepository!.ReadCheeps(limit, 0);

        // Assert
        Assert.Equal(limit, cheeps.Count);

        await Dispose(_context);
    }

    [Fact]
    public async Task ReadCheeps_SkipsOffsetCheeps()
    {
		ICheepRepository _cheepRepository; 
        IAuthorRepository _authorRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _authorRepository, _) = await GetContext();
        // Arrange
        int offset = 1;

        // Act
        var cheeps = await _cheepRepository!.ReadCheeps(-1, offset);

        // Assert
        var allCheeps = await _context!.Cheeps.ToListAsync();
        allCheeps.Reverse();
        var expectedCheeps = allCheeps.Skip(offset).ToList();
        Assert.Equal(expectedCheeps.Count, cheeps.Count);
        Assert.Equal(expectedCheeps.Select(c => c.Id), cheeps.Select(c => c.Id));

        await Dispose(_context);
    }

    [Fact]
    public async Task ReadCheeps_FiltersByAuthorNameRegex()
    {
		ICheepRepository _cheepRepository; 
        IAuthorRepository _authorRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _authorRepository, _) = await GetContext();
        // Arrange:
        string name = "Helge"; // Assuming there are authors with names starting with 'Helge'

        // Act
        var cheeps = await _cheepRepository!.ReadCheeps(-1, 0, name);

        // Assert
        Assert.All(cheeps, c => Assert.Equal(name, c.Author!.UserName!));

        await Dispose(_context);
    }


    [Fact]
    public async Task UpdateCheep_ChangesCheepText()
    {
		ICheepRepository _cheepRepository; 
        CheepDbContext _context; 
        (_context, _cheepRepository, _, _) = await GetContext();
        // Arrange
        var cheep = _context!.Cheeps.First();
        var newText = "Updated text";

        // Act
        await _cheepRepository!.UpdateCheep(cheep.Id, newText);

        // Assert
        var updatedCheep = await _context!.Cheeps.FindAsync(cheep.Id);
        Assert.Equal(newText, updatedCheep!.Text!);

        await Dispose(_context);
    }

    [Fact]
    public async Task UpdateCheep_NonExistentCheep_DoesNotThrow()
    {
        ICheepRepository _cheepRepository; 
        IAuthorRepository _authorRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _authorRepository, _) = await GetContext();
        // Arrange
        int nonExistentId = 999;
        var newText = "Should not work";

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _cheepRepository!.UpdateCheep(nonExistentId, newText)); 
        Assert.Null(exception);

        await Dispose(_context);
    }

    [Fact]
    public async Task CanFindAuthorbyName() 
    {
        CheepDbContext _context;
        IAuthorRepository _authorRepository;
        (_context, _, _authorRepository, _) = await GetContext();
        //Act
        var foundAuthor = await _authorRepository!.FindAuthorByName("Roger_Histand"); 

        //Assert
        Assert.Equal("Roger_Histand@hotmail.com", foundAuthor?.Email);

        await Dispose(_context);
    }

    [Fact]
    public async Task CanFindAuthorbyEmail() 
    {
        CheepDbContext _context;
        IAuthorRepository _authorRepository;
        (_context, _, _authorRepository, _) = await GetContext();
        //Act
        var foundAuthor = await _authorRepository!.FindAuthorByEmail("Roger_Histand@hotmail.com"); 

        //Assert
        Assert.Equal("Roger_Histand", foundAuthor?.UserName);  

        await Dispose(_context);
    }

    [Fact]
    public async Task Test_GetAuthorsFollowing() 
    {
		ICheepRepository _cheepRepository; 
        IAuthorRepository _authorRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _authorRepository, _) = await GetContext();
        //Arrange
        await MakeAFollowB("Helge", "Adrian",  _context); 

        //Act 
        ICollection<Author> HelgeFollowers = await _authorRepository!.GetAuthorsFollowing("Helge"); 

        //Assert
        var adrian = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Adrian");
        Assert.Contains(adrian, HelgeFollowers);

        await Dispose(_context);
    }

    [Fact]
    public async Task Test_AddingFollower() 
    {
		ICheepRepository _cheepRepository; 
        IAuthorRepository _authorRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _authorRepository, _) = await GetContext();
        // act
        await _authorRepository!.AddOrRemoveFollower("Adrian", "Helge");

        // assert 
        var adrian = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Adrian");
        var helge = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Helge");

        Assert.Contains(helge, adrian!.FollowingList!);

        await Dispose(_context);
    }

    [Fact]
    public async Task Test_RemovingFollower() 
    {
		ICheepRepository _cheepRepository; 
        IAuthorRepository _authorRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _authorRepository, _) = await GetContext();
        //arrange 
        await MakeAFollowB("Helge", "Adrian",  _context); 

        // act 
        await _authorRepository!.AddOrRemoveFollower("Helge", "Adrian");

        // assert 
        var adrian = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Adrian");
        var helge = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Helge");

        Assert.DoesNotContain(adrian, helge!.FollowingList!);

        await Dispose(_context);
    }

    [Fact]
    public async Task Test_GetPrivateTimelineCheeps() 
    {
		ICheepRepository _cheepRepository; 
        IAuthorRepository _authorRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _authorRepository, _) = await GetContext();
        //arrange 
        await MakeAFollowB("Helge", "Quintin_Sitts",  _context); 
        await MakeAFollowB("Helge", "Jacqualine_Gilcoine",  _context); 

        // act 
        var followingCheeps = await _cheepRepository!.GetPrivateTimelineCheeps("Helge", -1, 0); 

        // assert 
        var checkCheeps = from cheep in _context!.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                     where cheep.Author!.UserName! == "Quintin_Sitts" || cheep.Author!.UserName! == "Jacqualine_Gilcoine" || cheep.Author!.UserName! == "Helge"
                     orderby cheep.Id descending
                     select cheep; 
        
        await _context!.SaveChangesAsync(); 

        foreach(var cheep in checkCheeps.ToList()) {
            Assert.Contains(cheep, followingCheeps); 
        }

        await Dispose(_context);
    }

    private async Task MakeAFollowB(string aUsrname, string bUsrname, CheepDbContext _context) 
    {
        var query = 
            from a in _context!.Users 
            where a.UserName == aUsrname 
            select a; 

        var userToFollow = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == bUsrname);

        await query.ForEachAsync(user => {
            user.FollowingList ??= new List<Author>(); // make sure it isn't null
            if(user.FollowingList.Contains(userToFollow!)) { return; }
            (user.FollowingList ?? throw new Exception("Fucking followinglist is null")).Add(userToFollow!);
            });
        await _context!.SaveChangesAsync();
    } 
    
    [Fact]
    public async Task DeleteAuthorByName()
    {
		ICheepRepository _cheepRepository; 
        IAuthorRepository _authorRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _authorRepository, _) = await GetContext();
        // Arrange
        var author = new Author
        {
            UserName = "TestUser",
            Email = "testuser@example.com",
            Cheeps = new List<Cheep>()
        };

        var cheep1 = new Cheep { Text = "First cheep", TimeStamp = DateTime.UtcNow, Author = author };
        var cheep2 = new Cheep { Text = "Second cheep", TimeStamp = DateTime.UtcNow, Author = author };

   
        author.Cheeps.Add(cheep1);
        author.Cheeps.Add(cheep2);

        // Add the author and their cheeps to the database
        await _context!.Users.AddAsync(author);
        await _context!.SaveChangesAsync();

        // Act
        var deletedAuthor = await _authorRepository!.DeleteAuthorByName("TestUser");

        // Assert
        // Verify that the author is removed from the Users table
        Assert.NotNull(deletedAuthor);
        Assert.Null(await _context!.Users.FirstOrDefaultAsync(a => a.UserName == "TestUser"));

        // Verify that all cheeps belonging to the author are removed from the Cheeps table
        Assert.Empty(await _context!.Cheeps.Where(c => c.AuthorId == deletedAuthor.Id).ToListAsync());

        // Verify that the returned author matches the expected one
        Assert.Equal("TestUser", deletedAuthor.UserName);

        await Dispose(_context);
    }

    [Fact]
    public async Task TestMutualFollow() // ensure we can get followers and cheeps without problems from both 
    {
        CheepDbContext _context; 
        IAuthorRepository _authorRepository;
        ICheepRepository _cheepRepository; 
        (_context, _cheepRepository, _authorRepository, _) = await GetContext();
        // act
        await MakeAFollowB("Helge", "Adrian",  _context); 
        await MakeAFollowB("Adrian", "Helge",  _context); 

        // NOTE: a bit of code duplication from the assertions in the privatetimeline check and following checks but this is the only way we actually see mutual chirps
        //Assert
        ICollection<Author> HelgeFollowers = await _authorRepository!.GetAuthorsFollowing("Helge"); 
        ICollection<Author> AdrianFollowers = await _authorRepository!.GetAuthorsFollowing("Adrian"); 

        // assert, ensure that both are set up as eachothers followers
        var adrian = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Adrian");
        var helge = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Helge");
        Assert.Contains(adrian, HelgeFollowers);
        Assert.Contains(helge, AdrianFollowers);

        // assert, esure that boths private timelines contain all their cheeps
        var helgePrivateCheeps = await _cheepRepository!.GetPrivateTimelineCheeps("Helge", -1, 0); 
        var adrianPrivateCheeps = await _cheepRepository!.GetPrivateTimelineCheeps("Adrian", -1, 0); 

        var checkCheeps = from cheep in _context!.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                     where cheep.Author!.UserName! == "Adrian" || cheep.Author!.UserName! == "Helge"
                     orderby cheep.Id descending
                     select cheep; 
        

        foreach(var cheep in checkCheeps.ToList()) {
            Assert.Contains(cheep, helgePrivateCheeps); 
            Assert.Contains(cheep, adrianPrivateCheeps);
        }

        await Dispose(_context);
    }

    [Theory] // just make sure that no matter whether we getold or not everything works
    [InlineData("Test123", false)]
    [InlineData("Test123", true)]
    public async Task TestNotifyFollowNoTag(string cheepContent, bool getOld) 
    {
		ICheepRepository _cheepRepository; 
        IAuthorRepository _authorRepository;
        INotificationRepository _notificationRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _authorRepository, _notificationRepository) = await GetContext();
        await MakeAFollowB("Helge", "Adrian",  _context); 
        

        var adrian =await _authorRepository!.FindAuthorByName("Adrian");
        var helge =await _authorRepository!.FindAuthorByName("Helge");
        await _cheepRepository!.CreateCheep(new Cheep{
            Author=adrian,
            Text=cheepContent
        }); 
        // ensure that own cheeps don't show up
        await _cheepRepository!.CreateCheep(new Cheep{
            Author=helge,
            Text="very invalid cheep text"
        }); 

        var notifs = await _notificationRepository.GetNotifications("Helge", getOld); 

        Assert.True(notifs.All(n => {
            return n.cheep.Text == cheepContent && n.authorID == helge!.Id; 
            })); 

        await Dispose(_context);
    }

    
    [Theory] // idea here is tags should always show up and we should always know they are tag notifications
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
    public async Task TestNotifiedWhenTagged(string cheepContent, bool getOld, bool follow) 
    {
		ICheepRepository _cheepRepository; 
        IAuthorRepository _authorRepository;
        INotificationRepository _notificationRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _authorRepository, _notificationRepository) = await GetContext();


        var adrian = await _authorRepository!.FindAuthorByName("Adrian");
        var helge = await _authorRepository!.FindAuthorByName("Helge");

        if(follow) { await MakeAFollowB("Helge", "Adrian",  _context);  }
        
        await _cheepRepository!.CreateCheep(new Cheep{
            Author=adrian, Text=cheepContent
        }); 
        
        
        await _cheepRepository!.CreateCheep(new Cheep{
            Author=helge, Text="very invalid cheep text"
        }); 

        var notifs = await _notificationRepository.GetNotifications("Helge", getOld); 
        
        Assert.True(notifs.All(n => {
            return n.cheep.Text == cheepContent && n.authorID == helge!.Id && n.tagNotification; 
            }));

        await Dispose(_context);
    }

    [Theory] // idea here is make sure that no matter what combination of tags and no tags, only the new notification shows up 
    [InlineData("test cheep 123", "qwjeqjw")]
    [InlineData("test @Helge ", "qwewqeqw")]
    [InlineData("lalalaal", "qwe @Helge")]
    [InlineData("ejwjew @Helge", "qwe @Helge")]
    public async Task TestNotShowingOldNotifications(string cheepContent, string invalidCheepContent)
    {
		ICheepRepository _cheepRepository; 
        IAuthorRepository _authorRepository;
        INotificationRepository _notificationRepository;
        CheepDbContext _context; 
        (_context, _cheepRepository, _authorRepository, _notificationRepository) = await GetContext();
        await MakeAFollowB("Helge", "Adrian",  _context); 
        

        var adrian =await _authorRepository!.FindAuthorByName("Adrian");
        var helge =await _authorRepository!.FindAuthorByName("Helge");

        // ensure that own cheeps don't show up
        await _cheepRepository!.CreateCheep(new Cheep{
            Author=adrian,
            Text=invalidCheepContent
        }); 

        await _notificationRepository.GetNotifications("Helge", false);

        await _cheepRepository!.CreateCheep(new Cheep{
            Author=adrian,
            Text=cheepContent
        }); 

        var notifs = await _notificationRepository.GetNotifications("Helge", false); 

        Assert.True(notifs.All(n => {
            return n.cheep.Text == cheepContent && n.authorID == helge!.Id; 
            })); 

        await Dispose(_context);
    }
}