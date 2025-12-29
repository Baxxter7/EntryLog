using EntryLog.Business.Interfaces;
using Microsoft.Extensions.Options;

namespace EntryLog.Business.Cryptography;

internal class Argon2PasswordHasherService : IPasswordHasherService
{
    private readonly Argon2PasswordHashOptions _options;
    public Argon2PasswordHasherService(IOptions<Argon2PasswordHashOptions> options)
    {
        _options = options.Value;
    }
    public string Hash(string password)
    {
        throw new NotImplementedException();
    }

    public bool Verify(string password, string hash)
    {
        throw new NotImplementedException();
    }
}
