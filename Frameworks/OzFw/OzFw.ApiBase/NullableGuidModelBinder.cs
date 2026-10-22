using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OzFw.ApiBase
{
    /// <summary>
    /// Query/route'tan boş string olarak gelen Guid? değerlerini null'a bağlar.
    /// Kayıt: services.AddControllers(o => o.ModelBinderProviders.Insert(0, new NullableGuidModelBinderProvider()));
    /// </summary>
    public class NullableGuidModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            else if (Guid.TryParse(value, out var guid))
            {
                bindingContext.Result = ModelBindingResult.Success(guid);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Geçersiz GUID biçimi.");
            }
            return Task.CompletedTask;
        }
    }

    public class NullableGuidModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            return context.Metadata.ModelType == typeof(Guid?) ? new NullableGuidModelBinder() : null;
        }
    }
}
