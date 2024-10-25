using Microsoft.EntityFrameworkCore;
using Chirp.Core;
using Chirp.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class CheepDBContext(DbContextOptions<CheepDBContext> options) : IdentityDbContext<ChirpUser>(options)
{
    public DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Author> Authors { get; set; }
}