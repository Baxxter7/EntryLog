using EntryLog.Business.DTOs;

namespace EntryLog.Business.Interfaces;
public interface IFaceIdService
{
    Task<(bool success, string message, EmployeeFaceIdDto? data)> CreateEmployeeFaceIdAsync(AddEmployeeFaceIdDto faceIdDto);
    Task<EmployeeFaceIdDto> GetFaceIdAsync(int employeeCode);
    Task<string> GenerateReferenceImageTokenAsync(string userId);
    Task<string> GetReferenceImageAsync(string authHeader);
}
