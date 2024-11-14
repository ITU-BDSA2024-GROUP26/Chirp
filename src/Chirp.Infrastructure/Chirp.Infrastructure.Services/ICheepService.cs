using Chirp.Core.DTOs;

namespace Chirp.Infrastructure.Services;

using Core;

public interface ICheepService
{
    public Task<IEnumerable<CheepDTO>> GetCheepsAsync(int page, string? authorRegex=null);
}