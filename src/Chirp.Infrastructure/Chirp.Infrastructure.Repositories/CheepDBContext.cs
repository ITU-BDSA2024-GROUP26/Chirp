using Microsoft.EntityFrameworkCore;
using Chirp.Core;
using Chirp.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class CheepDbContext(DbContextOptions<CheepDbContext> options) : IdentityDbContext<Author>(options)
{
    public DbSet<Cheep> Cheeps { get; set; }
}