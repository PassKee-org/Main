using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PassKee.Business.Clients.Smtp.Core;
using PassKee.Business.Extensions;

namespace PassKee.Business.Clients.Smtp;

public class SmtpClientService : ISmtpClientService
{
    private readonly SmtpSettings _smtpSettings;

    private readonly MailAddress _defaultFromAddress;
    private readonly NetworkCredential _credentials;
    private readonly ILogger<SmtpClientService> _logger;

    private readonly char[] _separators = ";".ToCharArray();

    public SmtpClientService(
        IConfiguration configuration, 
        ILogger<SmtpClientService> logger
    )
    {
        _smtpSettings = new SmtpSettings(configuration);
        _defaultFromAddress = new MailAddress(_smtpSettings.EmailFrom, _smtpSettings.UserNameFrom);
        _credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password);
        _logger = logger;
    }

    private void ParseEmails(MailAddressCollection collection, string? emails)
    {
        if (string.IsNullOrWhiteSpace(emails))
            return;

        var emailArray = emails.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
        foreach (var em in emailArray)
        {
            collection.Add(em);
        }
    }

    public string SendEmail(
        string to,
        EmailBuilder emailBuilder,
        string? bcc
    )
    {
        try
        {
            emailBuilder.Build();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return e.Message;
        }
        return SendEmail(null, to, emailBuilder.Subject, emailBuilder.Body, null, bcc);
    }

    public string SendEmail(
        string to,
        string subject,
        string body,
        string? bcc
    )
    {
        return SendEmail(null, to, subject, body, null, bcc);
    }

    public string SendEmail(
        string? from,
        string to,
        string subject,
        string body,
        string? cc,
        string? bcc
    )
    {
        _logger.LogDebug($"Send email to {to} from {from} with subject {subject.MySubstring(0, 15)}");
        
        string res = string.Empty;
        var message = new MailMessage();

        message.From = string.IsNullOrWhiteSpace(from) ? _defaultFromAddress : new MailAddress(from);
        ParseEmails(message.ReplyToList, message.From?.Address);

        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("'to' parameter is required");

        ParseEmails(message.To, to);
        ParseEmails(message.CC, cc);
        ParseEmails(message.Bcc, bcc);

        message.Subject = subject.RemoveNewLineSymbols();
        message.IsBodyHtml = true;
        message.Body = body;

        message.Priority = MailPriority.Normal;

        SendViaSmtpClient(message);
        return res;
    }

    private void SendViaSmtpClient(MailMessage message)
    {
        var smtpClient = new System.Net.Mail.SmtpClient(_smtpSettings.Server);
        smtpClient.Port = _smtpSettings.Port;
        smtpClient.Credentials = _credentials;
        smtpClient.EnableSsl = _smtpSettings.EnableSsl;

        try
        {
            smtpClient.Send(message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
}

