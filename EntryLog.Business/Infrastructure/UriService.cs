using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.Infrastructure;

internal class UriService
{
    private readonly IHttpContextAccessor _httpContext;

    public UriService(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }
}
