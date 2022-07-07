using MailDelivery.Data;
using MailDelivery.Models;
using MailDelivery.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailDelivery.Services
{
    public class SqliteRepository : IDbRepository
    {
        private readonly AppDbContext context;

        public SqliteRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<int> CreateDistributionAsync(Distribution distribution)
        {
            await context.Distributions.AddAsync(distribution);
            await context.SaveChangesAsync();

            return distribution.Id;
        }

        public async Task<Distribution> FindDistributionAsync(int distributionId)
        {
            return await context.Distributions.FindAsync(distributionId);
        }

        public async Task<Template> FindTemplateAsync(MessageBuilderType type)
        {
            return await context.Templates.SingleAsync(x => x.MessageType == type);
        }

        public async Task<List<Letter>> GetUnsentLettersAsync()
        {
            return await context.Letters
                .Where(letter => letter.Status == StatusType.Registered)
                .Include(x => x.Distribution)
                .ThenInclude(x => x.Template)
                .ToListAsync();
        }

        public async Task MarkDistributionAsync(int distributionId, StatusType status, string reason = null)
        {
            var distribution = await context.Distributions.FindAsync(distributionId);
            distribution.Status = status;

            if (!string.IsNullOrEmpty(reason))
            {
                distribution.Reason = reason;
            }

            if (status == StatusType.Done)
            {
                distribution.StartedAt = await context.Letters
                    .Where(x => x.DistributionId == distributionId)
                    .MinAsync(x => x.SendingAt);
                distribution.EndedAt = DateTime.Now;
            }

            await context.SaveChangesAsync();
        }

        public async Task MarkLetterAsync(int letterId, StatusType status, string reason = null)
        {
            var letter = await context.Letters.FindAsync(letterId);
            letter.Status = status;

            if (!string.IsNullOrEmpty(reason))
            {
                letter.Reason = reason;
            }

            if (status == StatusType.Sending)
            {
                letter.SendingAt = DateTime.Now;
            }
            else if (status == StatusType.Done)
            {
                letter.SentAt = DateTime.Now;
            }

            await context.SaveChangesAsync();
        }

        public async Task RegisterLettersAsync(List<Letter> letters)
        {
            if (letters.Any())
            {
                await context.Letters.AddRangeAsync(letters);
                await context.SaveChangesAsync();
            }
        }
    }
}