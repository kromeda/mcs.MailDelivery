using MailDelivery.Models;
using System;

namespace MailDelivery.Exceptions
{
    public class LetterValidationException : System.Exception
    {
        public Letter Letter { get; set; }

        public LetterValidationException() { }

        public LetterValidationException(string message) : base(message) { }

        public LetterValidationException(string message, Exception inner) : base(message, inner) { }
    }
}