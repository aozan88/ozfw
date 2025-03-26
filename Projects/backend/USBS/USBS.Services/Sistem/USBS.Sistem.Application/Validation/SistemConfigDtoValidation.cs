using FluentValidation;
using Sistem.Application.DtoModels;

namespace Sistem.Application.Validations
{
    public class SistemConfigDtoValidation : AbstractValidator<SistemConfigDto>
    {
        public SistemConfigDtoValidation()
        {
            RuleFor(k => k.SistemConfigAdi)
                .NotEmpty()
                .Length(3, 100);

            RuleFor(k => k.SistemConfigTipiId)
                .NotEmpty();

            RuleFor(k => k.Durum)
                .InclusiveBetween(0, 1);
        }
    }
}
