using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace MoviesMaxAPI.Helpers
{
    public class TypeBinder<T> : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var propertyName = bindingContext.ModelName;
            var value = bindingContext.ValueProvider.GetValue(propertyName);    //get d value that comes in the property
            
            //if no value was sent then there is nothing we have to bind
            if (value == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }
            else
            {
                // if there is a vlaue, we desrialize that value & we use a Nuget package called Newtonsoft.Json
                /**We are deserializing to what type? we allow the model decide d type, so we use Generics <T> which allows us to
                 * pass types as parameters of each model/entity we want to deseialize have their different types**/
                try
                {
                    var deserializedValue = JsonConvert.DeserializeObject<T>(value.FirstValue);
                    bindingContext.Result = ModelBindingResult.Success(deserializedValue);
                }
                catch
                {
                    bindingContext.ModelState.TryAddModelError(propertyName, "The given value is not of the correct type");
                }                
                return Task.CompletedTask;
            }
        }
    }
}
