// Services/IEmailSenderService.cs
using System.Threading.Tasks;

namespace CarWorkshopManagementSystem.Services
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string toEmail, string subject, string message, byte[]? attachmentBytes = null, string? attachmentFileName = null);
    }
}