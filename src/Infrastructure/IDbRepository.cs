using Core;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure;

public interface IDbRepository
{
    public Task SeedAsync();
    public Task ResetAsync();
}