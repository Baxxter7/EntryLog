namespace EntryLog.Business.Mailtrap.Models;

internal class MailtrapApiOptions
{
    public string ApiUrl { get; init; } = string.Empty;
    public string ApiToken{ get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    public List<MailTrapTemplete> templates { get; init; } = new List<MailTrapTemplete>();
}

internal class MailTrapTemplete
{
    public string Uuid { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}