using Chirp.Core;
using Chirp.Core.Entities;
using Chirp.Infrastructure;
using Chirp.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace Chirp.CheepRepository.Tests;

public class CheepRepositoryTests : IDisposable
{

    private readonly ICheepRepository _repository;
    private readonly CheepDBContext _context;


    public CheepRepositoryTests()
    {
        // Set up In-Memory Database
        var options = new DbContextOptionsBuilder<CheepDBContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

        _context = new CheepDBContext(options);
        DbInitializer.SeedDatabase(_context);

        _repository = new Chirp.Infrastructure.Repositories.CheepRepository(_context);
    }


    public void Dispose()
    {
        _context.Dispose();
    }


    [Fact]
    public async Task CreateCheep_AddsCheepToDatabase()
    {
        // Arrange
        const string text = "Hello world";
        var newCheep = new Cheep
        {
            Text = text,
            Author = _context.Authors.First() // Assuming Authors are seeded
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
        // Arrange: Clear existing data to ensure a clean test environment
        _context.Cheeps.RemoveRange(_context.Cheeps);
        _context.Authors.RemoveRange(_context.Authors);
        await _context.SaveChangesAsync();

        // Create a new author
        Author author1 = new Author()
        {
            AuthorId = 2,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Cheeps = new List<Cheep>()
        };
        _context.Authors.Add(author1);
        await _context.SaveChangesAsync();

        // Create cheeps associated with the author
        Cheep c1 = new Cheep()
        {
            CheepId = 1,
            AuthorId = author1.AuthorId,
            Author = author1,
            Text = "First cheep",
            TimeStamp = DateTime.UtcNow
        };
        Cheep c2 = new Cheep()
        {
            CheepId = 2,
            AuthorId = author1.AuthorId,
            Author = author1,
            Text = "Second cheep",
            TimeStamp = DateTime.UtcNow
        };
        _context.Cheeps.AddRange(c1, c2);
        await _context.SaveChangesAsync();

        // Act
        var cheepList = await _repository.ReadCheeps(-1, 0, "");
        var cheepAmount = cheepList.Count;

        // Assert
        Assert.Equal(2, cheepAmount);
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
        var expectedCheeps = allCheeps.Skip(offset).ToList();
        Assert.Equal(expectedCheeps.Count, cheeps.Count);
        Assert.Equal(expectedCheeps.Select(c => c.CheepId), cheeps.Select(c => c.CheepId));
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
        await _repository.UpdateCheep(cheep.CheepId, newText);

        // Assert
        var updatedCheep = await _context.Cheeps.FindAsync(cheep.CheepId);
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

    [Fact]
    public async Task IsAuthorCreated() 
    {

        //Arrange 
        Author newAuthor = new Author()
        {
            AuthorId = 13,
            Name = "JoJo",
            Email = "jojo_daBeast.com",
            Cheeps = new List<Cheep>()
        };

        //Act
        await _repository.CreateAuthor(newAuthor); 

        //Assert
        var createdAuthor = await _context.Authors.FindAsync(newAuthor.AuthorId);
        Assert.Equal(newAuthor.Name, createdAuthor.Name);
        Assert.Equal(newAuthor.Email, createdAuthor.Email);
    }

    [Fact]
    public async Task CanFindAuthorbyName() 
    {
        
        //Act
        var foundAuthor = await _repository.FindAuthorbyName("Roger Histand"); 

        //Assert
        Assert.Equal("Roger+Histand@hotmail.com", foundAuthor?.Email);
    }

    [Fact]
    public async Task CanFindAuthorbyEmail() 
    {
        //Act
        var foundAuthor = await _repository.FindAuthorbyEmail("Roger+Histand@hotmail.com"); 

        //Assert
        Assert.Equal("Roger Histand", foundAuthor?.Name);  
    }

}