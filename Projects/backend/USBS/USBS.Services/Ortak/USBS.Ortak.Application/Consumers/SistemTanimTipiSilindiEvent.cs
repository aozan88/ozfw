using OzFwEventBus;

namespace USBS.Ortak.API.Consumers
{
    public class SistemTanimTipiSilindiEvent : IEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        DateTime IEvent.CreationDate => DateTime.Now;

        public Guid SistemTanimTipiId { get; set; }
        public OzFwBaseModels.Enums.DurumEnum YeniDurum { get; set; }

    }
}
