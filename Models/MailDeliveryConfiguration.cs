namespace MailDelivery.Models
{
    public class MailDeliveryConfiguration
    {
        public string SeqHttpHost { get; set; }

        public string SeqApiKey { get; set; }

        public HttpHosts Hosts { get; set; }

        public MailOptions Mail { get; set; }

        public class HttpHosts
        {
            public string MailMarkup { get; set; }
        }

        public class MailOptions
        {
            public bool SendEnabled { get; set; }

            public string ExchangeHost { get; set; }

            public string SenderLogin { get; set; }

            public string SenderPassword { get; set; }

            public int SmtpPort { get; set; }
        }
    }
}