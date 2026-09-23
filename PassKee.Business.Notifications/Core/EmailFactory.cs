using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using PassKee.Business.Clients.Smtp.Core;

namespace PassKee.Business.Notifications.Core;

public class EmailFactory
{
    private static readonly ConcurrentDictionary<string, EmailTemplateModel> _cachedTemplates = new();

    private string _layoutName = "_EmailLayout.htm";
    private readonly string _layoutTemplate;

    public EmailFactory()
    {
        _layoutTemplate = LoadLayoutFile();
    }

    public EmailTemplateModel GetEmailTemplate(string templateName)
    {
        if (_cachedTemplates.TryGetValue(templateName, out var res))
        {
            return res;
        }

        res = LoadEmailTemplate(templateName);
        return _cachedTemplates.GetOrAdd(templateName, res);
    }

    public EmailBuilder GetEmailBuilder(string templateName)
    {
        var et = GetEmailTemplate(templateName);
        return new EmailBuilder(et.BodyTemplate, et.SubjectTemplate);
    }

    private string LoadLayoutFile()
    {
        try
        {
            return LoadFile(_layoutName);
        }
        catch
        {
            return LoadFile("Layout.html");
        }
    }

    private string LoadFile(string templateName)
    {
        var assembly = GetType().Assembly;
        var localeCode = "en";
        var layoutResourcePath = $"{assembly.GetName().Name}.Templates.Emails.{localeCode}.{templateName}";
        var resource = assembly.GetManifestResourceStream(layoutResourcePath);
        if (resource == null)
        {
            throw new Exception($"Email template wasn't found: '{templateName}' at '{layoutResourcePath}'");
        }
        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }

    private EmailTemplateModel LoadEmailTemplate(string templateName)
    {
        var contentTemplate = LoadFile(templateName);
        string subjectTemplate = string.Empty;

        var subjectRegex = @"<!--\s*<subject>(?<subjectText>[^<]*)</subject>\s*-->";
        var subjectMatch = Regex.Match(contentTemplate, subjectRegex, RegexOptions.IgnoreCase);
        if (subjectMatch.Success && subjectMatch.Groups["subjectText"].Success)
        {
            subjectTemplate = subjectMatch.Groups["subjectText"].Value;
            contentTemplate = Regex.Replace(
                contentTemplate, 
                subjectRegex, 
                string.Empty, 
                RegexOptions.IgnoreCase
            );
        }

        return new EmailTemplateModel
        {
            BodyTemplate = _layoutTemplate.Replace("{body}", contentTemplate),
            SubjectTemplate = subjectTemplate
        };
    }
}

