using System;

namespace MailDelivery.Models
{
    public class Letter
    {
        public int Id { get; set; }

        public StatusType Status { get; set; }

        public string ToAddress { get; set; }

        public string Subject { get; set; }

        public string Arguments { get; set; }

        public DateTime? SendingAt { get; set; }

        public DateTime? SentAt { get; set; }

        public string Reason { get; set; }

        public int DistributionId { get; set; }
        public Distribution Distribution { get; set; }
    }
}