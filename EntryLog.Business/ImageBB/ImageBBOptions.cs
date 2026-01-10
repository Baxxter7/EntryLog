namespace EntryLog.Business.ImageBB;

internal record ImageBBOptions
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public int ExpirationSeconds { get; set; }
}