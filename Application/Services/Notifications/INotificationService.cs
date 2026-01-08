

namespace Application.Services.Notifications
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string message);
    }
}
