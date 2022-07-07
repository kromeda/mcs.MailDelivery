using MailDelivery.Models;
using MailDelivery.Models.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MailDelivery.Services
{
    public class MarkupClient : IMarkupClient
    {
        private readonly ILogger<MarkupClient> logger;
        private readonly MailDeliveryConfiguration configuration;
        private readonly HttpClient client;

        public MarkupClient(HttpClient client, IOptions<MailDeliveryConfiguration> options, ILogger<MarkupClient> logger)
        {
            this.client = client;
            this.configuration = options.Value;
            this.logger = logger;

            client.BaseAddress = new Uri(configuration.Hosts.Markup);
        }

        public async Task<string> RequestMarkupAsync(string route, string arguments)
        {
            var response = await client.PostAsync(route, new StringContent(arguments, Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Получен ответ с ошибкой от сервиса разметки MailMarkup. Сообщение: {Message}", content);
            }

            return response.IsSuccessStatusCode ? content : null;
        }
    }
}