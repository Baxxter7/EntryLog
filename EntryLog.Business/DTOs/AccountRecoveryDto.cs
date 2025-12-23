namespace EntryLog.Business.DTOs;

public record class AccountRecoveryDto
(
    string Token,
    string Password, 
    string PasswordConf
);
