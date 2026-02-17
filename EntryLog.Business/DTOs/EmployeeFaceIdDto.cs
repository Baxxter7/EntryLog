namespace EntryLog.Business.DTOs;

public record EmployeeFaceIdDto(
    string Base64Image,
    string RegisterDate,
    bool Active
);