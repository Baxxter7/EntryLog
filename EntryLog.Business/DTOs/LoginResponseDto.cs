namespace EntryLog.Business.DTOs;

public record class LoginResponseDto(
    int DocumentNumber,
    string Role,
    string Email, 
    string Name
);
