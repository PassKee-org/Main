using System.Collections.Generic;
using System.Linq;
using PassKee.Business.Clients.Smtp.Core;

namespace PassKee.Business.Clients.Smtp;

public class SmtpClientServiceMock : ISmtpClientService
{
    public List<FakeEmailModel> SentMessages = new();
    
    public bool IsEmailSent => SentMessages.Any();

    public void Reset()
    {
        SentMessages = new();
    }

    public string SendEmail(string to, EmailBuilder emailBuilder, string? bcc)
    {
        emailBuilder.Build();
        return SendEmail("", to, emailBuilder.Subject, emailBuilder.Body, null, bcc);
    }

    public string SendEmail(string to, string subject, string body, string? bcc)
    {
        return SendEmail("", to, subject, body, null, bcc);
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
        SentMessages.Add(new FakeEmailModel(from ?? "", to, subject, body, cc ?? "", bcc ?? ""));
        return "";
    }
}

