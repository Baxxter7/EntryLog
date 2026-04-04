namespace EntryLog.Web.Models;

public record CloseWorkSessionViewModel(
    string sessionId,
    string Latitude,
    string Longitude,
    IFormFile Image,
    string? Notes,
    string Descriptor
);
