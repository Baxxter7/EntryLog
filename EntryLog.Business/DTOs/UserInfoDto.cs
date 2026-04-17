namespace EntryLog.Business.DTOs;

public record UserInfoDto
(
    string Code,
    string Name,
    string Role,
    string Active,
    string Position,
    string PositionName,
    string PositionDescription,
    string Email,
    string CellPhone, 
    string DateOfBirthDay,
    string ProfileImage,
    string City,
    bool IsFaceIdActive,
    bool IsActive
);

