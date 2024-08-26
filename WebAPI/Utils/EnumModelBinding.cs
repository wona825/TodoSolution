using Infrasfructure.Error;
using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class EnumModelBinder<T> : IModelBinder where T : struct, Enum
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new CustomException(HttpStatusCode.InternalServerError, "An unexpected error occurred during model binding.");
        }

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var value = valueProviderResult.FirstValue;

        if (Enum.TryParse(typeof(T), value, true, out var result))
        {
            bindingContext.Result = ModelBindingResult.Success(result); 
        }
        else
        {
            throw new CustomException(HttpStatusCode.BadRequest, $"Invalid value '{value}' for parameter '{bindingContext.ModelName}'. Expected one of: {string.Join(", ", Enum.GetNames(typeof(T)))}.");
        }

        return Task.CompletedTask;
    }
}
