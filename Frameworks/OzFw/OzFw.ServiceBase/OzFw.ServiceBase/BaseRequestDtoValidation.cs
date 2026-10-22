using FluentValidation;
using OzFwBaseModels;

namespace OzFw.ServiceBase
{
    /// <summary>
    /// BaseRequestDto için RuleSet tabanlı doğrulama.
    /// RuleSet'ler: Common, GetAll, CreateOrUpdate. CreateOrUpdate, Payload'u DTO validator'ı ile (varsa) doğrular.
    /// </summary>
    public class BaseRequestDtoValidation<T> : AbstractValidator<BaseRequestDto<T>>
    {
        public const string Common = "Common";
        public const string GetAll = "GetAll";
        public const string CreateOrUpdate = "CreateOrUpdate";

        public BaseRequestDtoValidation(IValidator<T>? payloadValidator = null)
        {
            RuleSet(Common, () =>
            {
                RuleFor(x => x.ClientId).NotEmpty().WithMessage("ClientId boş olamaz.");
            });

            RuleSet(GetAll, () =>
            {
                RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Page 1'den küçük olamaz.");
                RuleFor(x => x.Size).GreaterThanOrEqualTo(-1).WithMessage("Size -1'den küçük olamaz.");
                RuleFor(x => x.Size).NotEqual(0).WithMessage("Size 0 olamaz (tüm kayıtlar için -1 kullanın).");
            });

            RuleSet(CreateOrUpdate, () =>
            {
                var payloadRule = RuleFor(x => x.Payload).NotNull().WithMessage("Payload boş olamaz.");
                if (payloadValidator != null)
                    payloadRule.SetValidator(payloadValidator!);
            });
        }
    }
}
