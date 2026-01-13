namespace EntryLog.Business.Mailtrap.Models;

internal class MailtrapApiOptions
{
    public string ApiUrl { get; init; } = string.Empty;
    public string ApiToken { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    public List<MailTrapTemplate> Templates { get; init; } = new List<MailTrapTemplate>();
}

internal class MailTrapTemplate
{
    public string Uuid { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}