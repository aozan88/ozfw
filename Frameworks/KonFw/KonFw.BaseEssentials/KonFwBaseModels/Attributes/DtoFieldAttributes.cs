namespace KonFwBaseModels
{
    /// <summary>
    /// Entity property'sinin DTO tarafındaki adı farklıysa kullanılır.
    /// Filtre/sıralama alan adları (DTO adı) bu eşlemeyle entity property'sine çevrilir.
    /// Adlar aynıysa attribute gerekmez.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DtoFieldAttribute : Attribute
    {
        public string DtoFieldName { get; }

        public DtoFieldAttribute(string dtoFieldName)
        {
            DtoFieldName = dtoFieldName;
        }
    }

    /// <summary>
    /// DTO property'si entity üzerinde iç içe bir yola karşılık geliyorsa kullanılır.
    /// Örn: [EntityField("TanimTipi.TanimTipiAdi")] public string TanimTipiAdi { get; set; }
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityFieldAttribute : Attribute
    {
        public string EntityPath { get; }

        public EntityFieldAttribute(string entityPath)
        {
            EntityPath = entityPath;
        }
    }
}
