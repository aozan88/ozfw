using FluentValidation;
using USBS.Common.Dtos.OrtakDtos;

namespace Ortak.Application.Validations
{
    public class KodDtoValidation : AbstractValidator<KodDto>
    {
        public KodDtoValidation()
        {
            RuleFor(k => k.KodAdi)
                .NotEmpty()
                .Length(3, 100);

            RuleFor(k => k.KodTipiId)
                .NotEmpty();

            RuleFor(k => k.Durum)
                .InclusiveBetween(0, 1);
        }
    }
}
