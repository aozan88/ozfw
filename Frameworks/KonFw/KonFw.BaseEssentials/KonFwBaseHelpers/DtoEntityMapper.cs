using System.Collections.Concurrent;
using System.Reflection;
using KonFwBaseModels;

namespace KonFwBaseHelpers
{
    /// <summary>
    /// İstemciden gelen filtre/sıralama alan adlarını (DTO adları) entity property yollarına çevirir.
    /// Öncelik: DTO üzerindeki [EntityField] > entity üzerindeki [DtoField] > aynı isimli property (büyük/küçük harf duyarsız).
    /// </summary>
    public static class DtoEntityMapper
    {
        private static readonly ConcurrentDictionary<(Type? Dto, Type Entity), IReadOnlyDictionary<string, string>> _cache = new();

        public static IReadOnlyDictionary<string, string> GetMappings(Type? dtoType, Type entityType)
        {
            return _cache.GetOrAdd((dtoType, entityType), key => BuildMappings(key.Dto, key.Entity));
        }

        /// <summary>
        /// Alan adını entity yoluna çevirir. Eşleme yoksa alan adı olduğu gibi döner
        /// (doğrudan "TanimTipi.TanimTipiAdi" gibi bir entity yolu verilmiş olabilir).
        /// </summary>
        public static string ResolveEntityPath(Type? dtoType, Type entityType, string field)
        {
            var mappings = GetMappings(dtoType, entityType);
            return mappings.TryGetValue(field, out var path) ? path : field;
        }

        public static void ClearCache() => _cache.Clear();

        private static IReadOnlyDictionary<string, string> BuildMappings(Type? dtoType, Type entityType)
        {
            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var entityProps = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in entityProps)
            {
                mappings.TryAdd(prop.Name, prop.Name);
            }

            foreach (var prop in entityProps)
            {
                var dtoField = prop.GetCustomAttribute<DtoFieldAttribute>();
                if (dtoField != null)
                    mappings[dtoField.DtoFieldName] = prop.Name;
            }

            if (dtoType != null)
            {
                foreach (var dtoProp in dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var entityField = dtoProp.GetCustomAttribute<EntityFieldAttribute>();
                    if (entityField != null)
                        mappings[dtoProp.Name] = entityField.EntityPath;
                }
            }

            return mappings;
        }
    }
}
