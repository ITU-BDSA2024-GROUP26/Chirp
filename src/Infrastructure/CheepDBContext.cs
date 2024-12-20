using Microsoft.EntityFrameworkCore;
using Core;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Infrastructure;

public class CheepDbContext(DbContextOptions<CheepDbContext> options) : IdentityDbContext<Author>(options)
{
    public static bool TestingSetup { get; set; } // decides whether we should manually migrate in code or not 
    public DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Notification> notifications { get; set; }
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

