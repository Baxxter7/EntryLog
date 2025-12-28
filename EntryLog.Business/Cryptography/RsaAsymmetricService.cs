using EntryLog.Business.Interfaces;
using Microsoft.Extensions.Options;

namespace EntryLog.Business.Cryptography;

internal class RsaAsymmetricService : IEncryptionService
{
    private readonly EncryptionKeyValues _encryptionKeyValues;

    public RsaAsymmetricService(IOptions<EncryptionKeyValues> options)
    {
        _encryptionKeyValues = options.Value;
    }

    public string Encrypt(string plainText)
    {
        throw new NotImplementedException();
    }

    public string Decrypt(string cipherText)
    {
        throw new NotImplementedException();
    }
}
public enum KeySize
{
    SIZE_512 = 512,
    SIZE_1024 = 1024,
    SIZE_2048 = 2048,
    SIZE_952 = 952,
    SIZE_1369 = 1369
}