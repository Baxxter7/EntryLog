using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.DTOs;

public record AddEmployeeFaceIdDto
(
    int EmployeeCode,
    IFormFile image,
    List<float> Descriptor
);
