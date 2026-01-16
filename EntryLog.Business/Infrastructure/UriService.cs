using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.Infrastructure;

internal class UriService
{
    private readonly IHttpContextAccessor _httpContext;

    public UriService(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;

        var request = (_httpContext.HttpContext?.Request) ?? 
            throw new Exception("Ha ocurrido un error al generar el contexto http de la aplicacion");
    }
}
