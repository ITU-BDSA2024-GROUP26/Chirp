using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Chirp.Razor;

public class CheepDBContext
{
    DbSet<Cheep> Cheeps { get; set; }
    DbSet<Author> Authors { get; set; }
    
}