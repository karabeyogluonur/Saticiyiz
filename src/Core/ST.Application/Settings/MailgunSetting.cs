

using ST.Domain.Entities.Common;
using ST.Domain.Interfaces;

namespace ST.Application.Settings
{
    public class MailgunSetting : IGroupSetting
    {
        public MailgunSetting()
        {
            ApiKey = "8a8f507eec4e4ae77e0a33008f93e45e-556e0aa9-47b7ace3";
            Domain = "saticiyiz.com";
            FromAddress = "postmaster@saticiyiz.com";
            FromName = "Satıcıyız";
            SendMessageRequestUrl = "https://api.mailgun.net/v3/{0}/messages";
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