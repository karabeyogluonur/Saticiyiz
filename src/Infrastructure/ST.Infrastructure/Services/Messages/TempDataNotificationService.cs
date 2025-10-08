using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ST.Application.DTOs.Messages;
using ST.Application.Interfaces.Messages;

namespace ST.Infrastructure.Services.Messages;

public class TempDataNotificationService : INotificationService
{
    private const string TempDataKey = "notifications";
    private readonly ITempDataDictionary _tempData;

    public TempDataNotificationService(ITempDataDictionaryFactory factory, IHttpContextAccessor contextAccessor)
    {
        _tempData = factory.GetTempData(contextAccessor.HttpContext!);
    }

    private Task AddAsync(string type, string message, string? title = null)
    {
        List<UINotificationDto> notifications = GetAllAsync().Result.ToList();
        notifications.Add(new UINotificationDto { Type = type, Message = message, Title = title });
        _tempData[TempDataKey] = JsonSerializer.Serialize(notifications);
        return Task.CompletedTask;
    }

    public Task SuccessAsync(string message, string? title = null) => AddAsync("success", message, title);
    public Task ErrorAsync(string message, string? title = null) => AddAsync("error", message, title);
    public Task WarningAsync(string message, string? title = null) => AddAsync("warning", message, title);
    public Task InfoAsync(string message, string? title = null) => AddAsync("info", message, title);

    public Task<IEnumerable<UINotificationDto>> GetAllAsync()
    {
        if (_tempData.TryGetValue(TempDataKey, out var obj) && obj is string json)
        {
            return Task.FromResult(JsonSerializer.Deserialize<List<UINotificationDto>>(json) ?? new List<UINotificationDto>().AsEnumerable());
        }
        return Task.FromResult(Enumerable.Empty<UINotificationDto>());
    }
}

