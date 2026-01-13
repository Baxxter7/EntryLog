using EntryLog.Business.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace EntryLog.Business.Cryptography;

internal class RsaAsymmetricEncryptionService : IEncryptionService
{
    private readonly EncryptionKeyValues _keys;
    private readonly RSACryptoServiceProvider _csp = new RSACryptoServiceProvider((int)KeySize.SIZE_2048);

    public RsaAsymmetricEncryptionService(IOptions<EncryptionKeyValues> options)
    {
        _keys = options.Value;
    }

    public string Encrypt(string plainText)
    {
        try
        {
            _csp.FromXmlString(_keys.PublicKey);
            byte[] data = Encoding.Unicode.GetBytes(plainText);
            byte[] cipher = _csp.Encrypt(data, true);
            return Convert.ToBase64String(cipher);
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Encryption failed.", ex);
        }
        finally
        {
            _csp.PersistKeyInCsp = false;
        }
    }

    public string Decrypt(string cipherText)
    {
        try
        {
            _csp.FromXmlString(_keys.PrivateKey);
            byte[] data = Convert.FromBase64String(cipherText);
            byte[] textPlain = _csp.Decrypt(data, true);
            return Encoding.Unicode.GetString(textPlain);
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Encryption failed.", ex);
        }
        finally
        {
            _csp.PersistKeyInCsp = false;
        }
    }
}
public enum KeySize
{
    SIZE_512 = 512,
    SIZE_1024 = 1024,
    SIZE_2048 = 2048
}