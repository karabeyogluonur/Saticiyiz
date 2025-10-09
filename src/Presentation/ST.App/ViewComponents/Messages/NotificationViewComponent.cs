using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ST.Application.DTOs.Messages;
using ST.Application.Interfaces.Messages;

namespace ST.App.ViewComponents.Messages;

public class NotificationViewComponent : ViewComponent
{
    private readonly INotificationService _notificationService;

    public NotificationViewComponent(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        IEnumerable<UINotificationDto> notifications = await _notificationService.GetAllAsync();
        return View(notifications);
    }
}
