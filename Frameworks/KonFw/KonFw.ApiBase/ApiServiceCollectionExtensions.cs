using KonFwBaseHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace KonFw.ApiBase
{
    public static class ApiServiceCollectionExtensions
    {
        /// <summary>
        /// BaseController tabanlı API'ler için AddControllers + framework varsayılanları:
        /// - Nullable olmayan referans tiplerine otomatik [Required] eklenmez; zorunluluk kuralları FluentValidation'da tanımlanır.
        /// - Guid? için boş string null'a bağlanır (query/route ve JSON gövde).
        /// </summary>
        public static IMvcBuilder AddKonFwControllers(this IServiceCollection services, Action<Microsoft.AspNetCore.Mvc.MvcOptions>? configure = null)
        {
            return services
                .AddControllers(options =>
                {
                    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                    options.ModelBinderProviders.Insert(0, new NullableGuidModelBinderProvider());
                    configure?.Invoke(options);
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new EmptyStringToNullableGuidConverter());
                });
        }
    }
}
