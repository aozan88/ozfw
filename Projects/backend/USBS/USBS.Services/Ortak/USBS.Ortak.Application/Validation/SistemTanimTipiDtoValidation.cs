using FluentValidation;
using USBS.Common.Dtos.OrtakDtos;

namespace Ortak.Application.Validations
{
    public class SistemTanimTipiDtoValidation : AbstractValidator<SistemTanimTipiDto>
    {
        public SistemTanimTipiDtoValidation()
        {
            RuleFor(k => k.TanimTipiAdi)
                .NotEmpty()
                .Length(3, 100);

            RuleFor(k => k.Durum)
                .InclusiveBetween(0, 1);
        }
    }
}
