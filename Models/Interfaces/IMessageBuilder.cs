using System.Net.Mail;
using System.Threading.Tasks;

namespace MailDelivery.Models.Interfaces
{
    public interface IMessageBuilder
    {
        Task<MailMessage> GenerateAsync(Letter letter);
    }
}
