using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

public class TokenManager : ITokenManager
{
    private readonly IMemoryCache _memoryCache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // IDEA: use distributed cache
    public TokenManager(IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
    {
        _memoryCache = memoryCache;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsCurrentActiveToken()
        => IsActive(GetCurrent());

    public void DeactivateCurrent()
        => Deactivate(GetCurrent());

    public bool IsActive(string token)
        => _memoryCache.Get(GetKey(token)) is null;

    public void Deactivate(string token)
        => _memoryCache.Set(GetKey(token), " ",
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // TODO: use jwt options
                });

    private string GetCurrent()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["authorization"];

        return StringValues.IsNullOrEmpty(authorizationHeader) ?
            string.Empty :
            authorizationHeader.Single().Split(" ").Last();
    }

    private static string GetKey(string token) => $"tokens:{token}:deactivated";
}
