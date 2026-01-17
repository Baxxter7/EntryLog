using EntryLog.Business.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.Infrastructure;

internal class UriService : IUriService
{
    private readonly IHttpContextAccessor _httpContext;

    public UriService(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;

        var request = (_httpContext.HttpContext?.Request) ??
            throw new InvalidOperationException("Ha ocurrido un error al generar el contexto http de la aplicacion");

        ApplicationURL = $"{request.Scheme}://{request.Host}{request.PathBase}";
        UserAgent = $"{request.Headers["Sec-Ch-Ua"]}";
        Platform = $"{request.Headers["Sec-Ch-Ua-Platform"]}";
        RemoteIpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown RemoteIpAddress";

    }

    public string ApplicationURL { get; private set; }
    public string UserAgent { get; private set; }
    public string Platform { get; private set; }
    public string RemoteIpAddress { get; private set; }
}