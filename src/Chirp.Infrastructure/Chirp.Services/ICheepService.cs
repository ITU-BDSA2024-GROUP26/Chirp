using Chirp.DTOs;

namespace Chirp.Services;

using Core;

public interface ICheepService
{
    public Task<IEnumerable<CheepDTO>> GetCheepsAsync(int page, string authorRegex=".*");
}