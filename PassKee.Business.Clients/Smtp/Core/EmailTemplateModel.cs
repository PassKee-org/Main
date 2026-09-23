namespace PassKee.Business.Clients.Smtp.Core
{
    public class EmailTemplateModel
    {
        // this class is stored in a template cache

        public string BodyTemplate { get; set; } // already includes the LAYOUT TEMPL + CONTENT TEMPL, both still have {placeholders}
        public string SubjectTemplate { get; set; } // subject is extracted from CONTENT TEMPL <subject> tag
    }
}
