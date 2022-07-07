using FluentValidation;
using Models;
using Models.MailDelivery;

namespace MailDelivery.Validators
{
    public class DistributionArgsValidator : AbstractValidator<DistributionArgs>
    {
        public DistributionArgsValidator()
        {
            RuleFor(x => x.SenderAddress)
                .NotEmpty()
                .WithMessage("Отсутствует почтовый адрес отправителя.");

            RuleFor(x => x.Template)
                .NotEqual(MessageBuilderType.None)
                .WithMessage("Не выбран шаблон письма.");
        }
    }
}
