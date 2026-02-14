using Microsoft.IdentityModel.Tokens;

namespace EntryLog.Business.JWT;

internal static class JwtValidator
{
    public static bool CustomLifeTimeValidator(DateTime? notBefore,
        DateTime? expires,
        SecurityToken securityToken,
        TokenValidationParameters validationParameters)
    {
        var now = DateTime.UtcNow;

        if (expires != null && expires <= now)
            return false;

        if(notBefore != null && notBefore > now)
            return false;

        return true;
    }
}
