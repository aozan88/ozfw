using FluentValidation;
using Sistem.API.ViewModels;

namespace Sistem.API.Validations
{
    public class SistemConfigValidation : AbstractValidator<SistemConfigViewModel>
    {
        public SistemConfigValidation()
        {
            RuleFor(k => k.SistemConfigAdi)
                .NotEmpty()
                .WithMessage("SistemConfigAdi cannot be empty.");

            RuleFor(k => k.SistemConfigTipiId)
                .NotEmpty()
                .WithMessage("SistemConfigTipiId cannot be empty.");

            RuleFor(k => k.Durum)
                .InclusiveBetween(0, 1)
                .WithMessage("Durum must be either 0 or 1.");
        }
    }
}
