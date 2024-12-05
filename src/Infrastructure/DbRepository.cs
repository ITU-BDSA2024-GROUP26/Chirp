using Core;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure;

public class DbRepository(CheepDbContext context, UserManager<Author> userManager) : IDbRepository
{
    public async Task SeedAsync()
    {
        await DbInitializer.SeedDatabase(context, userManager);
    }

    public async Task ResetAsync()
    {
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
}