using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Tests.Helpers;

internal sealed class AppUserBuilder
{
    private Guid _id = Guid.NewGuid();
    private int _code = 1001;
    private string _name = "John Doe";
    private RoleType _role = RoleType.Employee;
    private string _email = "john@test.com";
    private string _cellPhone = "55551234";
    private string _password = "hashed_password";
    private int _attempts;
    private string? _recoveryToken;
    private bool _recoveryTokenActive;
    private FaceID? _faceId;
    private bool _active = true;

    public AppUserBuilder WithId(Guid id) { _id = id; return this; }
    public AppUserBuilder WithCode(int code) { _code = code; return this; }
    public AppUserBuilder WithName(string name) { _name = name; return this; }
    public AppUserBuilder WithRole(RoleType role) { _role = role; return this; }
    public AppUserBuilder WithEmail(string email) { _email = email; return this; }
    public AppUserBuilder WithCellPhone(string cellPhone) { _cellPhone = cellPhone; return this; }
    public AppUserBuilder WithPassword(string password) { _password = password; return this; }
    public AppUserBuilder WithAttempts(int attempts) { _attempts = attempts; return this; }
    public AppUserBuilder WithRecoveryToken(string? token) { _recoveryToken = token; return this; }
    public AppUserBuilder WithRecoveryTokenActive(bool active) { _recoveryTokenActive = active; return this; }
    public AppUserBuilder WithFaceId(FaceID? faceId) { _faceId = faceId; return this; }
    public AppUserBuilder WithActive(bool active) { _active = active; return this; }

    public AppUser Build() => new()
    {
        Id = _id,
        Code = _code,
        Name = _name,
        Role = _role,
        Email = _email,
        CellPhone = _cellPhone,
        Password = _password,
        Attempts = _attempts,
        RecoveryToken = _recoveryToken,
        RecoveryTokenActive = _recoveryTokenActive,
        FaceID = _faceId is null ? null : new FaceID
        {
            ImageUrl = _faceId.ImageUrl,
            RegisterDate = _faceId.RegisterDate,
            Descriptor = [.. _faceId.Descriptor],
            Active = _faceId.Active
        },
        Active = _active
    };
}
