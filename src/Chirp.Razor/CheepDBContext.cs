using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Chirp.Razor;

public class CheepDBContext : DbContext
{
    DbSet<Cheep> Cheeps { get; set; }
    DbSet<Author> Authors { get; set; }

    public CheepDBContext(DbContextOptions<CheepDBContext>options): base(options) {}
    
}