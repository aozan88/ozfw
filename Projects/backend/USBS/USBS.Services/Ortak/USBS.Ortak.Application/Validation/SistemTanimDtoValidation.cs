using FluentValidation;
using USBS.Common.Dtos.OrtakDtos;

namespace Ortak.Application.Validations
{
    public class SistemTanimDtoValidation : AbstractValidator<SistemTanimDto>
    {
        public SistemTanimDtoValidation()
        {
            RuleFor(k => k.TanimAdi)
                .NotEmpty()
                .Length(3, 100);

            RuleFor(k => k.TanimTipiId)
                .NotEmpty();

            RuleFor(k => k.Durum)
                .InclusiveBetween(0, 1);
        }
    }
}
