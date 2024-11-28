using Core;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;


namespace Repository.Tests;

public class RepositoryTests : IAsyncLifetime
{
    private ICheepRepository? _cheepRepository;
    private IAuthorRepository? _authorRepository; 
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
    }
    
    public async Task DisposeAsync()
    {
        await _context!.DisposeAsync();
    }


    [Fact]
    public async Task CreateCheep_AddsCheepToDatabase()
    {
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
    }

    [Fact]
    public async Task ReadCheeps_ReturnsAllCheeps()
    {
        var expectedNumCheeps = _context!.Cheeps.Count();
        var cheeps = await _cheepRepository!.ReadCheeps(-1, 0);
        var actualNumCheeps = cheeps.Count;
    
        // Assert
        Assert.Equal(expectedNumCheeps, actualNumCheeps);
    }


    [Fact]
    public async Task ReadCheeps_ReturnsLimitedCheeps()
    {
        // Arrange
        int limit = 2;

        // Act
        var cheeps = await _cheepRepository!.ReadCheeps(limit, 0);

        // Assert
        Assert.Equal(limit, cheeps.Count);
    }

    [Fact]
    public async Task ReadCheeps_SkipsOffsetCheeps()
    {
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
    }

    [Fact]
    public async Task ReadCheeps_FiltersByAuthorNameRegex()
    {
        // Arrange:
        string name = "Helge"; // Assuming there are authors with names starting with 'Helge'

        // Act
        var cheeps = await _cheepRepository!.ReadCheeps(-1, 0, name);

        // Assert
        Assert.All(cheeps, c => Assert.Equal(name, c.Author!.UserName!));
    }


    [Fact]
    public async Task UpdateCheep_ChangesCheepText()
    {
        // Arrange
        var cheep = _context!.Cheeps.First();
        var newText = "Updated text";

        // Act
        await _cheepRepository!.UpdateCheep(cheep.Id, newText);

        // Assert
        var updatedCheep = await _context!.Cheeps.FindAsync(cheep.Id);
        Assert.Equal(newText, updatedCheep!.Text!);
    }

    [Fact]
    public async Task UpdateCheep_NonExistentCheep_DoesNotThrow()
    {
        // Arrange
        int nonExistentId = 999;
        var newText = "Should not work";

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _cheepRepository!.UpdateCheep(nonExistentId, newText)); 
        Assert.Null(exception);
    }

    [Fact]
    public async Task CanFindAuthorbyName() 
    {
        //Act
        var foundAuthor = await _authorRepository!.FindAuthorByName("Roger Histand"); 

        //Assert
        Assert.Equal("Roger+Histand@hotmail.com", foundAuthor?.Email);
    }

    [Fact]
    public async Task CanFindAuthorbyEmail() 
    {
        //Act
        var foundAuthor = await _authorRepository!.FindAuthorByEmail("Roger+Histand@hotmail.com"); 

        //Assert
        Assert.Equal("Roger Histand", foundAuthor?.UserName);  
    }

    [Fact]
    public async Task Test_GetAuthorsFollowing() 
    {
        //Arrange
        await MakeAFollowB("Helge", "Adrian"); 

        //Act 
        ICollection<Author> HelgeFollowers = await _authorRepository!.GetAuthorsFollowing("Helge"); 

        //Assert
        var adrian = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Adrian");
        Assert.Contains(adrian, HelgeFollowers);
    }

    [Fact]
    public async Task Test_AddingFollower() 
    {
        // act
        await _authorRepository!.AddOrRemoveFollower("Adrian", "Helge");

        // assert 
        var adrian = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Adrian");
        var helge = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Helge");

        Assert.Contains(helge, adrian!.FollowingList!);
    }

    [Fact]
    public async Task Test_RemovingFollower() 
    {
        //arrange 
        await MakeAFollowB("Helge", "Adrian");

        // act 
        await _authorRepository!.AddOrRemoveFollower("Helge", "Adrian");

        // assert 
        var adrian = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Adrian");
        var helge = await _context!.Users.FirstOrDefaultAsync(a=> a.UserName == "Helge");

        Assert.DoesNotContain(adrian, helge!.FollowingList!);
    }

    [Fact]
    public async Task Test_GetPrivateTimelineCheeps() 
    {
        //arrange 
        await MakeAFollowB("Helge", "Quintin Sitts");
        await MakeAFollowB("Helge", "Jacqualine Gilcoine"); 

        // act 
        var followingCheeps = await _cheepRepository!.GetPrivateTimelineCheeps("Helge", -1, 0); 

        // assert 
        var checkCheeps = from cheep in _context!.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                     where cheep.Author!.UserName! == "Quintin Sitts" || cheep.Author!.UserName! == "Jacqualine Gilcoine" || cheep.Author!.UserName! == "Helge"
                     orderby cheep.Id descending
                     select cheep; 
        
        await _context!.SaveChangesAsync(); 

        foreach(var cheep in checkCheeps.ToList()) {
            Assert.Contains(cheep, followingCheeps); 
        }
    }

    private async Task MakeAFollowB(string aUsrname, string bUsrname) 
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
    }

    [Fact]
    public async Task TestMutalFollow() // ensure we can get followers and cheeps without problems from both 
    {
        await MakeAFollowB("Helge", "Adrian"); 
        await MakeAFollowB("Adrian", "Helge");

        await Test_GetAuthorsFollowing(); 
        await Test_GetPrivateTimelineCheeps(); 
    }
}