using Microsoft.EntityFrameworkCore;
using Core;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Infrastructure;

/// <summary>
/// Class that represents our database context.
/// Note that inheriting from IdentityDbContext<Author> takes care of the Author class, including all the Identity stuff 
/// </summary>
/// <param name="options">An objects object which allows configuring of the database, e.g. which database provider to use</param>
public class CheepDbContext(DbContextOptions<CheepDbContext> options) : IdentityDbContext<Author>(options)
{
    public DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Notification> notifications { get; set; }
    // We override this method to ensure that the notification class gets the proper primary key and foreign keys. 
    // Which is important since that particular composite primary key guarantees the functional requirement of notifications being one to many(Cheep to Author as well as Author to Author)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>()
            .HasKey(n => new { n.cheepID, n.authorID });
        
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.cheep)
            .WithMany() // Define the relationship, if any
            .HasForeignKey(n => n.cheepID); // Assuming Cheep has a primary key

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.authorToNotify)
            .WithMany() // Define the relationship, if any
            .HasForeignKey(n => n.authorID); // Assuming Author has a primary key
    }
}

