using EntryLog.Entities.Enums;

namespace EntryLog.Entities.POCOEntities;

public class AppUser
{
    public Guid Id { get; set; }
    public int Code { get; set; }
    public string Name { get; set; }
    public RoleType Role { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CellPhone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public string? RecoveryToken { get; set; } = string.Empty;
    public bool RecoveryTokenActive { get; set; }
    public bool Active { get; set; }
}
