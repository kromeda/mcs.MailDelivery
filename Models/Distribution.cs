using System;
using System.Collections.Generic;

namespace MailDelivery.Models
{
    public class Distribution
    {
        public int Id { get; set; }

        public string FromAddress { get; set; }

        public string FromAlias { get; set; }

        public StatusType Status { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public string Reason { get; set; }

        public int TemplateId { get; set; }
        public Template Template { get; set; }

        public ICollection<Letter> Letters { get; set; }
    }
}