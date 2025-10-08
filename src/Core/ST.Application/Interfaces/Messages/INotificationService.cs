using ST.Application.DTOs.Messages;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.Application.Interfaces.Messages
{
    public interface INotificationService
    {
        Task SuccessAsync(string message, string? title = null);
        Task ErrorAsync(string message, string? title = null);
        Task WarningAsync(string message, string? title = null);
        Task InfoAsync(string message, string? title = null);
        Task<IEnumerable<UINotificationDto>> GetAllAsync();
    }
}