using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.DTOs;

public record CloseWorkSessionDto(
   string? SessionId,
   string UserId,
   string Latitude,
   string Longitude,
   IFormFile Image,
   string? Notes,
   string Descriptor,
   string Country,
   string City,
   string Neighbourhood
);