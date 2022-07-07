using MailDelivery.Exceptions;
using MailDelivery.Models;
using MailDelivery.Models.Interfaces;
using System;
using System.Linq;
using System.Net.Mail;

namespace MailDelivery.Validators
{
    public class LetterValidator : ILetterValidator
    {
        public void Check(Letter letter)
        {
            if (letter == null)
            {
                throw new LetterValidationException("Отсутствует объект-письмо");
            }

            if (string.IsNullOrEmpty(letter.ToAddress))
            {
                throw new LetterValidationException("Отсутствует адрес получателя") { Letter = letter };
            }

            try
            {
                MailAddress address = new MailAddress(letter.ToAddress);
                bool addressMatches = letter.ToAddress
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(adr => adr.Trim())
                    .Contains(address.Address);

                if (!addressMatches)
                {
                    throw new LetterValidationException("Почтовый адрес не соответствует") { Letter = letter };
                }
            }
            catch (FormatException fe)
            {
                throw new LetterValidationException("Не удалось прочитать почтовый адрес", fe) { Letter = letter };
            }
        }
    }
}
