using Chirp.Core;
using Chirp.Core.Entities;
using Chirp.Infrastructure;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;


namespace Chirp.CheepRepository.Tests;

public class CheepRepositoryTests : IAsyncLifetime
{
    private ICheepRepository _repository;
    private CheepDbContext _context;
    
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
        
        _repository = new Chirp.Infrastructure.Repositories.CheepRepository(_context);
    }
    
    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }


    [Fact]
    public async Task CreateCheep_AddsCheepToDatabase()
    {
        // Arrange
        const string text = "Hello world";
        var newCheep = new Cheep
        {
            Text = text,
            Author = _context.Users.First() // Assuming Authors are seeded
        };
    
        // Act
        await _repository.CreateCheep(newCheep);
    
        // Assert
        var cheeps = await _context.Cheeps.ToListAsync();
        Assert.Equal(text, cheeps.Last().Text);
    }

    [Fact]
    public async Task ReadCheeps_ReturnsAllCheeps()
    {
        var expectedNumCheeps = _context.Cheeps.Count();
        var cheeps = await _repository.ReadCheeps(-1, 0, "");
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
        var cheeps = await _repository.ReadCheeps(limit, 0, "");

        // Assert
        Assert.Equal(limit, cheeps.Count);
    }

    [Fact]
    public async Task ReadCheeps_SkipsOffsetCheeps()
    {
        // Arrange
        int offset = 1;

        // Act
        var cheeps = await _repository.ReadCheeps(-1, offset, "");

        // Assert
        var allCheeps = await _context.Cheeps.ToListAsync();
        allCheeps.Reverse();
        var expectedCheeps = allCheeps.Skip(offset).ToList();
        Assert.Equal(expectedCheeps.Count, cheeps.Count);
        Assert.Equal(expectedCheeps.Select(c => c.Id), cheeps.Select(c => c.Id));
    }

    [Fact]
    public async Task ReadCheeps_FiltersByAuthorNameRegex()
    {
        // Arrange:
        string regex = "^Helge"; // Assuming there are authors with names starting with 'Helge'

        // Act
        var cheeps = await _repository.ReadCheeps(-1, 0, regex);

        // Assert
        Assert.All(cheeps, c => Assert.Matches(regex, c.Author!.Name!));
    }


    [Fact]
    public async Task UpdateCheep_ChangesCheepText()
    {
        // Arrange
        var cheep = _context.Cheeps.First();
        var newText = "Updated text";

        // Act
        await _repository.UpdateCheep(cheep.Id, newText);

        // Assert
        var updatedCheep = await _context.Cheeps.FindAsync(cheep.Id);
        Assert.Equal(newText, updatedCheep!.Text!);
    }

    [Fact]
    public async Task UpdateCheep_NonExistentCheep_DoesNotThrow()
    {
        // Arrange
        int nonExistentId = 999;
        var newText = "Should not work";

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _repository.UpdateCheep(nonExistentId, newText)); 
        Assert.Null(exception);
    }

    // [Fact]
    // public async Task IsAuthorCreated() 
    // {
    //     //Arrange 
    //     Author newAuthor = new Author()
    //     {
    //         Id = 13,
    //         Name = "JoJo",
    //         Email = "jojo_daBeast.com",
    //         Cheeps = new List<Cheep>()
    //     };
    //
    //     //Act
    //     await _repository.CreateAuthor(newAuthor); 
    //
    //     //Assert
    //     var createdAuthor = await _context.Authors.FindAsync(newAuthor.Id);
    //     Assert.Equal(newAuthor.Name, createdAuthor.Name);
    //     Assert.Equal(newAuthor.Email, createdAuthor.Email);
    // }

    [Fact]
    public async Task CanFindAuthorbyName() 
    {
        //Act
        var foundAuthor = await _repository.FindAuthorByName("Roger Histand"); 

        //Assert
        Assert.Equal("Roger+Histand@hotmail.com", foundAuthor?.Email);
    }

    [Fact]
    public async Task CanFindAuthorbyEmail() 
    {
        //Act
        var foundAuthor = await _repository.FindAuthorByEmail("Roger+Histand@hotmail.com"); 

        //Assert
        Assert.Equal("Roger Histand", foundAuthor?.Name);  
    }
}