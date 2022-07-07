using MailDelivery.Models;
using MailDelivery.Models.Interfaces;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MailDelivery.Services.MessageGenerators
{
    public class AttachlessMessageAdapter : IMessageBuilder
    {
        private readonly IMarkupClient client;

        public AttachlessMessageAdapter(IMarkupClient client)
        {
            this.client = client;
        }

        public async Task<MailMessage> GenerateAsync(Letter letter)
        {
            string body;

            if (!letter.Distribution.Template.BodyIsHtml)
            {
                body = letter.Distribution.Template.TextBody;
            }
            else
            {
                body = await client.RequestMarkupAsync(letter.Distribution.Template.HttpRoute, letter.Arguments)
                    ?? throw new Exception("Не удалось получить разметку для письма.");
            }

            var receivers = letter.ToAddress.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var from = new MailAddress(letter.Distribution.FromAddress, letter.Distribution.FromAlias);
            var message = new MailMessage
            {
                From = from,
                Subject = letter.Subject,
                Body = body,
                IsBodyHtml = letter.Distribution.Template.BodyIsHtml,
            };

            foreach (var receiver in receivers)
            {
                message.To.Add(receiver);
            }

            return message;
        }
    }
}