using EntryLog.Business.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace EntryLog.Tests.Business.Cryptography;

public class RsaEncryptionServiceTests
{
    private readonly RsaAsymmetricEncryptionService _sut;

    public RsaEncryptionServiceTests()
    {
        using var rsa = new RSACryptoServiceProvider(2048);
        var publicKey = rsa.ToXmlString(false);
        var privateKey = rsa.ToXmlString(true);

        var options = Options.Create(new EncryptionKeyValues
        {
            PublicKey = publicKey,
            PrivateKey = privateKey
        });
        _sut = new RsaAsymmetricEncryptionService(options);
    }

    [Fact]
    public void Encrypt_ReturnsNonEmptyBase64String()
    {
        var cipher = _sut.Encrypt("hello world");

        cipher.Should().NotBeNullOrEmpty();
        var act = () => Convert.FromBase64String(cipher);
        act.Should().NotThrow();
    }

    [Fact]
    public void Decrypt_ReturnsOriginalPlainText()
    {
        var plainText = "test secret data";
        var cipher = _sut.Encrypt(plainText);

        var result = _sut.Decrypt(cipher);

        result.Should().Be(plainText);
    }

    [Fact]
    public void Decrypt_InvalidCipherText_ThrowsCryptographicException()
    {
        var act = () => _sut.Decrypt("not-valid-base64-cipher");

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Encrypt_DifferentInputs_ProduceDifferentOutputs()
    {
        var cipher1 = _sut.Encrypt("text1");
        var cipher2 = _sut.Encrypt("text2");

        cipher1.Should().NotBe(cipher2);
    }
}
