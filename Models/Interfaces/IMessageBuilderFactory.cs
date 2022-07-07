using Models;

namespace MailDelivery.Models.Interfaces
{
    public interface IMessageBuilderFactory
    {
        IMessageBuilder Get(MessageBuilderType adapterType);
    }
}