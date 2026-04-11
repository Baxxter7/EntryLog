using EntryLog.Business.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace EntryLog.Tests.Business.Cryptography;

public class Argon2PasswordHasherServiceTests
{
    private readonly Argon2PasswordHasherService _sut;

    public Argon2PasswordHasherServiceTests()
    {
        var options = Options.Create(new Argon2PasswordHashOptions
        {
            DegreeOfParallelism = 1,
            MemorySize = 1024,
            Iterations = 1,
            SaltSize = 16,
            HashSize = 32
        });
        _sut = new Argon2PasswordHasherService(options);
    }

    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = _sut.Hash("password123");

        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Hash_ContainsSaltAndHashSeparatedByColon()
    {
        var hash = _sut.Hash("password123");

        hash.Should().Contain(":");
        hash.Split(':').Should().HaveCount(2);
    }

    [Fact]
    public void Hash_DifferentPasswordsProduceDifferentHashes()
    {
        var hash1 = _sut.Hash("password1");
        var hash2 = _sut.Hash("password2");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Hash_SamePasswordProducesDifferentHashes()
    {
        var hash1 = _sut.Hash("password123");
        var hash2 = _sut.Hash("password123");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.Hash("password123");

        var result = _sut.Verify("password123", hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_IncorrectPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("password123");

        var result = _sut.Verify("wrong_password", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_InvalidHashFormat_ThrowsFormatException()
    {
        var act = () => _sut.Verify("password", "invalid_hash_no_colon");

        act.Should().Throw<FormatException>();
    }
}
