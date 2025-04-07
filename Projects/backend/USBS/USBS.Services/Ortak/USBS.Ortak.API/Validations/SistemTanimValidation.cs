using FluentValidation;
using Ortak.API.ViewModels;

namespace Ortak.API.Validations
{
    public class SistemTanimValidation : AbstractValidator<SistemTanimVM>, IValidator<SistemTanimVM>
    {
        public SistemTanimValidation()
        {
            RuleFor(k => k.TanimAdi)
                .NotEmpty()
                .WithMessage("SistemTanimAdi cannot be empty.");

            RuleFor(k => k.TanimTipiId)
                .NotEmpty()
                .WithMessage("SistemTanimTipiId cannot be empty.");

            RuleFor(k => k.Durum)
                .InclusiveBetween(0, 1)
                .WithMessage("Durum must be either 0 or 1.");
        }
    }
}
