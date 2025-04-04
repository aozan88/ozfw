using FluentValidation;
using Sistem.Application.DtoModels;

namespace Sistem.Application.Validations
{
    public class SistemParametreDtoValidation : AbstractValidator<SistemParametreDto>
    {
        public SistemParametreDtoValidation()
        {
            RuleFor(k => k.SistemParametreAdi)
                .NotEmpty()
                .Length(3, 100);

            RuleFor(k => k.SistemParametreTipiId)
                .NotEmpty();

            RuleFor(k => k.Durum)
                .InclusiveBetween(0, 1);
        }
    }
}
