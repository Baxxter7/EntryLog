namespace EntryLog.Business.DTOs;

public record GetWorkSessionDto
(
    string Id,
    int EmployeeId,
    GetCheckDto CheckIn,
    GetCheckDto? CheckOut,
    string? TotalWorked,
    string Status
);

public record GetCheckDto(
    string Method,
    string? DeviceName,
    string Date,
    GetLocationDto Location,
    string PhotoUrl,
    string? Notes
);

public record GetLocationDto(
   string Latitude,
   string Longitude,
   string IpAddress
 );
