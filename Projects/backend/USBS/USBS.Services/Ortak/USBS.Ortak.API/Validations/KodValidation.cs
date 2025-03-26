using FluentValidation;
using Ortak.API.ViewModels;

namespace Ortak.API.Validations
{
    public class KodValidation : AbstractValidator<KodVM>
    {
        public KodValidation()
        {
            RuleFor(k => k.KodAdi)
                .NotEmpty()
                .WithMessage("KodAdi cannot be empty.");

            RuleFor(k => k.KodTipiId)
                .NotEmpty()
                .WithMessage("KodTipiId cannot be empty.");

            RuleFor(k => k.Durum)
                .InclusiveBetween(0, 1)
                .WithMessage("Durum must be either 0 or 1.");
        }
    }
}
