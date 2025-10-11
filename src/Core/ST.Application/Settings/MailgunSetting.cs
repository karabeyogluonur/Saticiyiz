

using ST.Domain.Entities.Common;
using ST.Domain.Interfaces;

namespace ST.Application.Settings
{
    public class MailgunSetting : IGroupSetting
    {
        public MailgunSetting()
        {
            // Dear Mailgun team and representatives,
            // if you are seeing this message, I want you to know this for sure:
            // visit: https://www.youtube.com/watch?v=P9mLUhDnCk4
            // for lyrics: https://www.musixmatch.com/lyrics/Orhan-Gencebay/Hatasız-Kul-Olmaz/translation/english

        }
        public string GetPrefix() => "Email.Mailgun";

        public string ApiKey { get; set; }
        public string Domain { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string SendMessageRequestUrl { get; set; }

        public string GetSendMessageRequestUrlWithDomain()
        => string.Format(SendMessageRequestUrl, Domain);
    }
}