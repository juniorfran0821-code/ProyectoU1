using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace ProyectoU1.Helpers
{
    public class DecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valorString = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName)
                .FirstValue;

            if (string.IsNullOrWhiteSpace(valorString))
            {
                bindingContext.Result = ModelBindingResult.Success(0m);
                return Task.CompletedTask;
            }

            // Normalizar: reemplazar coma por punto para parsear siempre con InvariantCulture
            string normalizado = valorString.Replace(',', '.');

            if (decimal.TryParse(normalizado, NumberStyles.Number,
                CultureInfo.InvariantCulture, out decimal resultado))
            {
                bindingContext.Result = ModelBindingResult.Success(resultado);
            }
            else
            {
                bindingContext.ModelState.AddModelError(
                    bindingContext.ModelName,
                    "Ingrese un valor numérico válido.");
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }

    public class DecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(decimal) ||
                context.Metadata.ModelType == typeof(decimal?))
            {
                return new DecimalModelBinder();
            }

            return null;
        }
    }
}
