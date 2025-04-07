using FluentValidation;
using Sistem.API.ViewModels;

namespace Sistem.API.Validations
{
    public class SistemParametreValidation : AbstractValidator<SistemParametreVM>, IValidator<SistemParametreVM>
    {
        public SistemParametreValidation()
        {
            RuleFor(k => k.ParametreAdi)
                .NotEmpty()
                .WithMessage("SistemParametreAdi cannot be empty.");

            RuleFor(k => k.ParametreTipiId)
                .NotEmpty()
                .WithMessage("SistemParametreTipiId cannot be empty.");

            RuleFor(k => k.Durum)
                .InclusiveBetween(0, 1)
                .WithMessage("Durum must be either 0 or 1.");
        }
    }
}
