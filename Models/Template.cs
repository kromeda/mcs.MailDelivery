using Models;

namespace MailDelivery.Models
{
    public class Template
    {
        public int Id { get; set; }

        public MessageBuilderType MessageType { get; set; }

        public bool BodyIsHtml { get; set; }

        public string HttpRoute { get; set; }

        public string TextBody { get; set; }
    }
}