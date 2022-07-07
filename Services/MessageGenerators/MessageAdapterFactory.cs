using MailDelivery.Models.Interfaces;
using Models;
using System.Collections.Generic;
using System.Linq;

namespace MailDelivery.Services.MessageGenerators
{
    public class MessageAdapterFactory : IMessageBuilderFactory
    {
        private readonly IEnumerable<IMessageBuilder> adapters;

        public MessageAdapterFactory(IEnumerable<IMessageBuilder> adapters)
        {
            this.adapters = adapters;
        }

        public IMessageBuilder Get(MessageBuilderType adapterType)
        {
            return adapterType switch
            {
                MessageBuilderType.None => null,
                MessageBuilderType.SimpleAttachless => adapters.OfType<AttachlessMessageAdapter>().Single(),
                _ => null,
            };
        }
    }
}