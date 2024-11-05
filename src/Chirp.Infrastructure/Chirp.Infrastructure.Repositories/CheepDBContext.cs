using Microsoft.EntityFrameworkCore;
using Chirp.Core;
using Chirp.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class CheepDBContext(DbContextOptions<CheepDBContext> options) : IdentityDbContext<ChirpUser>(options)
{
    public static bool testingSetup { get; set; } // decides whether we should manually migrate in code or not 
    public DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Author> Authors { get; set; }
}