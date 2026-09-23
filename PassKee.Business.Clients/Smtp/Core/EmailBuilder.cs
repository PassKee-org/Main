using System.Net;

namespace PassKee.Business.Clients.Smtp.Core
{
    public class EmailBuilder
    {
        private TemplatedTextBuilder? _subjectBuilder;
        private TemplatedTextBuilder _bodyBuilder;

        public string Body = string.Empty;
        public string Subject = string.Empty;
        public EmailBuilder(string bodyTemplate, string? subjectTemplate = null)
        {
            _bodyBuilder = new TemplatedTextBuilder(bodyTemplate, 16384);
            if (!string.IsNullOrWhiteSpace(subjectTemplate))
                _subjectBuilder = new TemplatedTextBuilder(subjectTemplate, 4096);
        }

        public void AddPlaceholder(string key, string value)
        {
            _bodyBuilder.AddPlaceholder(key, value);
            _subjectBuilder?.AddPlaceholder(key, value); // note the ?. it is only called when obj is not null
        }

        public void Build()
        {
            // BODY
            _bodyBuilder.Build();
            Body = _bodyBuilder.Text;

            // SUBJECT
            if (_subjectBuilder != null)
            {
                _subjectBuilder.Build();
                // subject is plain text
                Subject = WebUtility.HtmlDecode(_subjectBuilder.Text);
            }
        }
    }
}
