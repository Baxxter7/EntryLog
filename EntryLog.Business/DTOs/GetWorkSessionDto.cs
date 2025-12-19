namespace EntryLog.Business.DTOs;

public record GetWorkSessionDto
(
    string Id,
    int EmployeeId,
    GetCheckDto CheckIn,
    GetCheckDto CheckOut,
    TimeSpan? TotalWorked,
    string Status
);

public record GetCheckDto(
    string Method,
    string? DeviceName,
    DateTime Date,
    GetLocationDto Location,
    string PhotoUrl,
    string? Notes
);

public record GetLocationDto(
   string Latitude,
   string Longitude,
   string IpAddress
 );