using PassKee.Business.Clients.Smtp.Core;

namespace PassKee.Business.Clients.Smtp;

public interface ISmtpClientService
{
    public string SendEmail(
        string to,
        EmailBuilder emailBuilder,
        string? bcc
    );

    public string SendEmail(
        string to,
        string subject,
        string body,
        string? bcc
    );

    public string SendEmail(
        string? from,
        string to,
        string subject,
        string body,
        string? cc,
        string? bcc
    );
}

