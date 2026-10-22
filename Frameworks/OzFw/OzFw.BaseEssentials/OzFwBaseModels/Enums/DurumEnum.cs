namespace OzFwBaseModels.Enums
{
    /// <summary>
    /// Kayıt durumu. Silme işlemleri soft-delete olarak Durum = Silindi ile yapılır.
    /// </summary>
    public enum DurumEnum
    {
        Silindi = -1,
        Pasif = 0,
        Aktif = 1
    }
}
