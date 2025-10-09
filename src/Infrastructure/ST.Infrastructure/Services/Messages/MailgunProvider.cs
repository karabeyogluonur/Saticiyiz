using System.Text;
using System.Text.Json;
using ST.Application.DTOs.Messages;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Messages;
using ST.Application.Settings;
using ST.Domain.Entities.Configurations;

namespace ST.Infrastructure.Services.Email;

public class MailgunProvider : IEmailProvider
{
    private readonly ISettingService _settingService;
    private readonly IHttpClientFactory _httpClientFactory;

    public MailgunProvider(ISettingService settingService, IHttpClientFactory httpClientFactory)
    {
        _settingService = settingService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task SendTemplateMailAsync(string to, string subject, string templateName, string templateDataJson)
    {
        MailgunSetting mailgunSetting = await _settingService.GetGlobalSettingsAsync<MailgunSetting>();

        if (string.IsNullOrEmpty(mailgunSetting.ApiKey) || string.IsNullOrEmpty(mailgunSetting.Domain))
        {
            throw new InvalidOperationException("Mailgun API anahtarı veya domain ayarları yapılandırılmamış.");
        }

        HttpClient client = _httpClientFactory.CreateClient("Mailgun");
        string requestUri = mailgunSetting.GetSendMessageRequestUrlWithDomain();

        FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("from", $"{mailgunSetting.FromName} <{mailgunSetting.FromAddress}>"),
            new KeyValuePair<string, string>("to", to),
            new KeyValuePair<string, string>("subject", subject),
            new KeyValuePair<string, string>("template", templateName),
            new KeyValuePair<string, string>("h:X-Mailgun-Variables", templateDataJson)
        });

        using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{mailgunSetting.ApiKey}")));
        httpRequest.Content = formContent;

        HttpResponseMessage response = await client.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Mailgun e-posta gönderilemedi. Status: {response.StatusCode}, Response: {errorContent}");
        }
    }
}