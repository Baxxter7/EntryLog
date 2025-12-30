namespace EntryLog.Business.Mailtrap.Models;

internal class MailtrapApiOptions
{
    public string ApiUrl { get; init; } = string.Empty;
    public string ApiToken{ get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
}
