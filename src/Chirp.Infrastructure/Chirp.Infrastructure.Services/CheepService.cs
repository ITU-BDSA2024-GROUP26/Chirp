using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;

namespace Chirp.Infrastructure.Services;

using System.Reflection;

public class CheepService(ICheepRepository cheepRepository) : ICheepService
{
    private const int MaxCheeps = 32;

    public async Task<IEnumerable<CheepDTO>> GetCheepsAsync(int page, string authorRegex=".*")
    {
        var cheeps = await cheepRepository.ReadCheeps(MaxCheeps, (page - 1) * MaxCheeps, authorRegex);
        return cheeps.Select(cheep => new CheepDTO(cheep));
    }
}
