using MailDelivery.Background;
using MailDelivery.Models.Interfaces;
using MailDelivery.Services;
using MailDelivery.Services.MessageGenerators;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Quartz;
using System;
using System.Net.Http;

namespace MailDelivery.Extenssions
{
    public static class IServiceCollectionExtenssions
    {
        public static IServiceCollection AddQuartzSendLetters(this IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                var key = new JobKey("Send letters");

                q.AddJob<SendLettersJob>(options => options.WithIdentity(key));
                q.AddTrigger(options => options
                    .ForJob(key)
                    .WithIdentity(key.Name + " trigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithInterval(TimeSpan.FromSeconds(30))
                        .RepeatForever()));
            });

            return services;
        }

        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient<IMarkupClient, MarkupClient>()
                .AddTransientHttpErrorPolicy(policy => AddWithRetry(policy));

            return services;
        }

        public static IServiceCollection AddMessageAdapters(this IServiceCollection services)
        {
            services.AddTransient<IMessageBuilderFactory, MessageAdapterFactory>();
            services.AddTransient<IMessageBuilder, AttachlessMessageAdapter>();

            return services;
        }

        private static AsyncPolicy<HttpResponseMessage> AddWithRetry(PolicyBuilder<HttpResponseMessage> policy)
        {
            return policy.WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5)
            });
        }
    }
}