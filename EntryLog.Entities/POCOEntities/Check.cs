namespace EntryLog.Entities.POCOEntities;

public class Check
{
    public string Method { get; set; } = string.Empty;
    public string? DeviceName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Location Location { get; set; } = new();
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Notes { get; set; } 
}
