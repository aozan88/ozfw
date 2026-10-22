using OzFwEventBus;
using Ortak.Application.Contract;

namespace USBS.Ortak.API.Consumers
{
    public class SistemTanimTipiSilindiEventHandler : IEventHandler<SistemTanimTipiSilindiEvent>
    {

        private readonly ISistemTanimRepository _repository;

        public SistemTanimTipiSilindiEventHandler(ISistemTanimRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(SistemTanimTipiSilindiEvent @event)
        {
            var tanimlar = _repository.GetAktifTanimByTanimTipiIdAsync(@event.SistemTanimTipiId);
              
            if(tanimlar != null && tanimlar.Result.Any())
            {
                foreach (var tanim in tanimlar.Result)
                {
                    tanim.Durum = @event.YeniDurum;
                }

                await _repository.UpdateManyAsync(tanimlar.Result);

                Console.WriteLine($"[EventHandler] SistemTanim güncellendi. TanimTipiId: {@event.SistemTanimTipiId}, YeniDurum: {@event.YeniDurum}");

                await Task.CompletedTask;
            }
        }
    }
}
