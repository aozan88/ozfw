using OzFwBaseModels.Enums;

namespace OzFw.BaseInfrastructure
{
    /// <summary>
    /// Bu arayüzü uygulayan entity'ler GenericRepository tarafından soft-delete edilir
    /// (Durum = Silindi) ve sorgularda silinmiş kayıtlar varsayılan olarak dışarıda bırakılır.
    /// </summary>
    public interface ISoftDeletable
    {
        DurumEnum Durum { get; set; }
    }
}
