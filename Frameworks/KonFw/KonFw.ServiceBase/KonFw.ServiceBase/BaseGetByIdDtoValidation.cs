using FluentValidation;
using KonFwBaseModels;

namespace KonFw.ServiceBase
{
    /// <summary>RuleSet'ler: Common, GetById.</summary>
    public class BaseGetByIdDtoValidation : AbstractValidator<BaseGetByIdDto>
    {
        public const string Common = "Common";
        public const string GetById = "GetById";

        public BaseGetByIdDtoValidation()
        {
            RuleSet(Common, () =>
            {
                RuleFor(x => x.ClientId).NotEmpty().WithMessage("ClientId boş olamaz.");
            });

            RuleSet(GetById, () =>
            {
                RuleFor(x => x.Payload).NotEmpty().WithMessage("Payload (Id) boş olamaz.");
            });
        }
    }
}
