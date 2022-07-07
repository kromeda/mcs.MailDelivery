using System.Threading.Tasks;

namespace MailDelivery.Models.Interfaces
{
    public interface IMarkupClient
    {
        Task<string> RequestMarkupAsync(string route, string arguments);
    }
}
