

namespace Domain.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string message);
    }
}
