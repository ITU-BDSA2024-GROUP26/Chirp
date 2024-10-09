using Microsoft.EntityFrameworkCore;
using Chirp.Core;
using Chirp.Core.Entities;

namespace Chirp.Infrastructure.Repositories;

public class CheepDBContext : DbContext
{
    public DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Author> Authors { get; set; }

    public CheepDBContext(DbContextOptions<CheepDBContext>options): base(options) {}
}