using Microsoft.EntityFrameworkCore;
using Chirp.Core;
using Chirp.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class CheepDbContext(DbContextOptions<CheepDbContext> options) : IdentityDbContext<Author>(options)
{
    public static bool testingSetup { get; set; } // decides whether we should manually migrate in code or not 
    public DbSet<Cheep> Cheeps { get; set; }
}