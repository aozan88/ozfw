using FluentValidation;
using Ortak.Application.DtoModels;

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
