namespace PassKee.Business.Clients.Smtp.Core
{
    public class EmailTemplateModel
    {
        // this class is stored in a template cache

        public string BodyTemplate { get; set; } = string.Empty;
        public string SubjectTemplate { get; set; } = string.Empty;
    }
}
