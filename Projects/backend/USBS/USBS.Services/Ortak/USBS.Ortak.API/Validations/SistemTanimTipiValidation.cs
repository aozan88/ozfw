using FluentValidation;
using Ortak.API.ViewModels;

namespace Ortak.API.Validations
{
    public class SistemTanimTipiValidation : AbstractValidator<SistemTanimTipiVM>, IValidator<SistemTanimTipiVM>
    {
        public SistemTanimTipiValidation()
        {
            RuleFor(k => k.TanimTipiAdi)
                .NotEmpty()
                .WithMessage("SistemTanimTipiAdi cannot be empty.");

            RuleFor(k => k.Durum)
                .InclusiveBetween(0, 1)
                .WithMessage("Durum must be either 0 or 1.");
        }
    }
}
