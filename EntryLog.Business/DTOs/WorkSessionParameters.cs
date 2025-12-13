using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.DTOs; 

public record WorkSessionParameters(
   string UserId,
   string Method,
   string DeviceName,
   string Latitude,
   string IpAddress,
   IFormFile Image,
   string? Notes
    );