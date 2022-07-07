using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MailDelivery.Models.Interfaces
{
    public interface IDbRepository
    {
        Task<int> CreateDistributionAsync(Distribution distribution);

        Task<Distribution> FindDistributionAsync(int distributionId);

        Task<Template> FindTemplateAsync(MessageBuilderType type);

        Task RegisterLettersAsync(IEnumerable<Letter> letters);

        Task MarkDistributionAsync(int distributionId, StatusType status, string reason = null);

        Task MarkLetterAsync(int letterId, StatusType status, string reason = null);

        Task<List<Letter>> GetUnsentLettersAsync();
    }
}