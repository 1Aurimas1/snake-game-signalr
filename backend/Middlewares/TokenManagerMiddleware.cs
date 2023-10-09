
public class TokenManagerMiddleware : IMiddleware
{
    private readonly ITokenManager _tokenManager;

    public TokenManagerMiddleware(ITokenManager tokenManager)
    {
        _tokenManager = tokenManager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_tokenManager.IsCurrentActiveToken())
        {
            await next(context);

            return;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }
}
