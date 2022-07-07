using MailDelivery.Models;
using MailDelivery.Models.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MailDelivery.Background
{
    [DisallowConcurrentExecution]
    public class SendLettersJob : IJob
    {
        private readonly ILogger<SendLettersJob> logger;
        private readonly IDbRepository repository;
        private readonly ILetterValidator validator;
        private readonly IMessageBuilderFactory factory;
        private readonly IServiceProvider services;
        private readonly MailDeliveryConfiguration.MailOptions options;

        private const int maxProcessCount = 4;
        private const int port = 345;

        public SendLettersJob(ILogger<SendLettersJob> logger,
                              IOptions<MailDeliveryConfiguration> options,
                              IDbRepository repository,
                              ILetterValidator validator,
                              IMessageBuilderFactory factory,
                              IServiceProvider services)
        {
            this.logger = logger;
            this.options = options.Value.Mail;
            this.repository = repository;
            this.validator = validator;
            this.factory = factory;
            this.services = services;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var letters = await repository.GetUnsentLettersAsync();
            var distributions = letters.GroupBy(letter => letter.DistributionId);

            foreach (var distribution in distributions)
            {
                try
                {
                    int lettersCount = distribution.Count();
                    int chunkSize = (lettersCount + maxProcessCount + 1) / maxProcessCount;
                    int processCount = lettersCount >= maxProcessCount ? maxProcessCount : lettersCount;
                    var chunks = Enumerable.Range(0, processCount).Select((_, index) => distribution.Skip(index * chunkSize).Take(chunkSize));
                    logger.LogInformation("Начинается рассылка №{DistributionId}. Количество писем: {LettersCount}.", distribution.Key, lettersCount);

                    var tasks = chunks.Select(chunk =>
                    {
                        return Task
                            .Run(async () => await SendChunk(chunk))
                            .ContinueWith(x =>
                            {
                                if (x.Status == TaskStatus.Faulted)
                                {
                                    logger.LogError(x.Exception, "Ошибка при обработке порции почтовых сообщений.");
                                }
                            });
                    });

                    await Task
                        .WhenAll(tasks)
                        .ContinueWith(async t => await AfterDistributionSent(t, distribution.Key));
                }
                catch (Exception ex)
                {
                    await repository.MarkDistributionAsync(distribution.Key, StatusType.Failed, ex.Message);
                    logger.LogError(ex, "Ошибка при попытке выполнить рассылку {DistributionId}", distribution.Key);
                }
            }
        }

        private async Task SendChunk(IEnumerable<Letter> chunk)
        {
            using var scope = services.CreateScope();
            IDbRepository scopedRepository = scope.ServiceProvider.GetRequiredService<IDbRepository>();

            using var client = new SmtpClient(options.ExchangeHost, port);
            client.Credentials = new NetworkCredential(options.SenderLogin, options.SenderPassword);

            foreach (var letter in chunk)
            {
                try
                {
                    await scopedRepository.MarkLetterAsync(letter.Id, StatusType.Sending);
                    validator.Check(letter);
                    var messageBuilder = factory.Get(letter.Distribution.Template.MessageType);
                    var message = await messageBuilder.GenerateAsync(letter);

                    if (options.SendEnabled)
                    {
                        await client.SendMailAsync(message);
                    }

                    await scopedRepository.MarkLetterAsync(letter.Id, StatusType.Done);
                }
                catch (Exception ex)
                {
                    await scopedRepository.MarkLetterAsync(letter.Id, StatusType.Failed, ex.Message);
                    logger.LogWarning(ex, "Ошибка при попытке отправить письмо {@Letter}", letter);
                }
            }
        }

        private async Task AfterDistributionSent(Task t, int distributionId)
        {
            if (t.Status == TaskStatus.RanToCompletion)
            {
                var distribution = await repository.FindDistributionAsync(distributionId);
                if (distribution.Status == StatusType.Transmited)
                {
                    await repository.MarkDistributionAsync(distributionId, StatusType.Done);
                    logger.LogInformation("Рассылка писем №{DistributionId} успешно завершена.", distributionId);
                }
            }
            else if (t.Status == TaskStatus.Faulted)
            {
                await repository.MarkDistributionAsync(distributionId, StatusType.Failed, t.Exception.Message);
                logger.LogError(t.Exception, "Рассылка писем №{DistributionId} завершена с ошибкой.", distributionId);
            }
            else
            {
                logger.LogWarning("Рассылка писем завершилась с непредвиденным статусом = {Status}", t.Status);
            }
        }
    }
}