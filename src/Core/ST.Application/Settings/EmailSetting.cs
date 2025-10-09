

using ST.Domain.Entities.Common;
using ST.Domain.Enums;
using ST.Domain.Interfaces;

namespace ST.Application.Settings
{
    public class EmailSetting : IGroupSetting
    {
        public EmailSetting()
        {
            Provider = EmailProvider.Mailgun;
        }
        public string GetPrefix() => "Email";

        public EmailProvider Provider { get; set; }

    }
}