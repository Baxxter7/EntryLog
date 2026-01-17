using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.DTOs;

public record CreateJobSessionDto(
   string UserId,
   string Latitude,
   string Longitude,
   IFormFile Image,
   string? Notes
);
