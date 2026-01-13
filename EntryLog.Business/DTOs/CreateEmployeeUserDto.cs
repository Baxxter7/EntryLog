namespace EntryLog.Business.DTOs;

public record CreateEmployeeUserDto
(
    string DocumentNumber,
    string UserName,
    string CellPhone,
    string Password,
    string PasswordConf
);