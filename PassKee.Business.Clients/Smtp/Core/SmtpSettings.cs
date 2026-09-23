using Microsoft.Extensions.Configuration;

namespace PassKee.Business.Clients.Smtp.Core
{
    class SmtpSettings
    {
        public string? Server { get; set; }
        public string? UserName { get; set; }
        public string? UserNameFrom { get; set; }
        public string EmailFrom { get; set; }
        public string? Password { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }

        public SmtpSettings(IConfiguration configuration)
        {
            Server = configuration.GetValue<string>("Smtp:Server");
            UserName = configuration.GetValue<string>("Smtp:UserName");
            Password = configuration.GetValue<string>("Smtp:Password");
            UserNameFrom = configuration.GetValue<string>("Smtp:From:Name");
            EmailFrom = configuration.GetValue<string>("Smtp:From:Email") 
                ?? throw new InvalidOperationException("Smtp:From:Email must be configured in appsettings.");
            Port = configuration.GetValue<int>("Smtp:Port");
            EnableSsl = configuration.GetValue<bool>("Smtp:EnableSsl");
        }
    }
}
