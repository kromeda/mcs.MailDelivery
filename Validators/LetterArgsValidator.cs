using FluentValidation;
using Models;

namespace MailDelivery.Validators
{
    public class LetterArgsValidator : AbstractValidator<LetterArgs>
    {
        public LetterArgsValidator()
        {
            RuleFor(x => x.Subject)
                .NotEmpty()
                .WithMessage("Необходимо указать тему письма.");

            RuleFor(x => x.ReceiverAddress)
                .NotEmpty()
                .WithMessage("Адрес получателя не может быть пуст.")
                .EmailAddress()
                .WithMessage("Не валидный адрес электронной почты.");
        }
    }
}