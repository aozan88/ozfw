using Ortak.Application.Contract;
using USBS.Common.Domain;
using USBS.Common.Dtos.OrtakDtos;
using OzFwBaseModels;
using OzFwBaseModels.Enums;
using OzFw.ServiceBase;
using USBS.Ortak.API.Consumers;
using OzFwEventBus;

namespace Ortak.Application.Business
{
    public class SistemTanimTipiServis : BaseService<SistemTanimTipi, SistemTanimTipiDto>, ISistemTanimTipiServis
    {
        private readonly ISistemTanimTipiRepository _repository;
        private readonly IEventBus _eventBus;

        public SistemTanimTipiServis(ISistemTanimTipiRepository repository, IEventBus eventBus) : base(repository)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        /// <summary>Tanım tipi silindiğinde bağlı tanımların da silinmesi için olay yayınlanır.</summary>
        public override async Task<BaseResponseDto<SistemTanimTipiDto>> DeleteAsync(Guid uid)
        {
            var result = await base.DeleteAsync(uid);
            if (result.IsSuccess)
            {
                await _eventBus.PublishAsync(new SistemTanimTipiSilindiEvent
                {
                    SistemTanimTipiId = uid,
                    YeniDurum = DurumEnum.Silindi
                });
            }
            return result;
        }
    }
}
