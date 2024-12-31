using Core;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure;

/// <summary>
/// A repository class to interface with the database for tasks that aren't merely concerned with Cheeps, Authors or Notifications, but rather the whole database. 
/// These commands are primarily useful for testing, to ensure consistency in UI-tests
/// </summary>
/// <param name="context">The database context to interface with</param>
/// <param name="userManager">The usermanager which is needed to create users when seeding the database.</param>
public class DbRepository(CheepDbContext context, UserManager<Author> userManager) : IDbRepository
{
    // A command to seed the database, a functional requirement. 
    public async Task SeedAsync()
    {
        await DbInitializer.SeedDatabase(context, userManager);
    }

    // A command to reset the database to it's initial(empty) state, used for UI-tests 
    public async Task ResetAsync()
    {
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
}