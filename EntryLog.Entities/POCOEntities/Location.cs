namespace EntryLog.Entities.POCOEntities;

public class Location
{
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Neighbourhood { get; set; } = string.Empty;
    public string? ApproximateAddress { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}
