namespace EntryLog.Business.Interfaces;
internal interface IEmailSenderService
{
    Task<bool> SendTextEmailAsync(string text, string to);
    Task<bool> SendEmailWithTemplateAsync(string templateName, string to, object? data = null);
}
