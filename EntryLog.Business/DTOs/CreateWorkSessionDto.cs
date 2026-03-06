using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.DTOs;

public record CreateWorkSessionDto(
   string UserId,
   string Latitude,
   string Longitude,
   IFormFile Image,
   string? Notes,
   string descriptor
);
