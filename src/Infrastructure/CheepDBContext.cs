using Microsoft.EntityFrameworkCore;
using Core;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Infrastructure;

public class CheepDbContext(DbContextOptions<CheepDbContext> options) : IdentityDbContext<Author>(options)
{
    public static bool TestingSetup { get; set; } // decides whether we should manually migrate in code or not 
    public DbSet<Cheep> Cheeps { get; set; }
}