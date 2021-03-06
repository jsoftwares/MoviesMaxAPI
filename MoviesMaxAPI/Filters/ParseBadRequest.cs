using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace MoviesMaxAPI.Filters
{
    public class ParseBadRequest : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var result = context.Result as IStatusCodeActionResult; //cast it to IStatusCodeActionResult
            if(result == null)
            {
                return;
            }

            var statusCode = result.StatusCode;
            if(statusCode == 400)
            {
                var response = new List<string>();
                var badRequestObjectResult = context.Result as BadRequestObjectResult;
                if(badRequestObjectResult.Value is string)
                {
                    response.Add(badRequestObjectResult.Value.ToString());
                }
                else if (badRequestObjectResult.Value is IEnumerable<IdentityError> errors)
                {
                    //add case for when Errors is of type IEnumerable
                    foreach (var error in errors)
                    {
                        response.Add(error.Description);    //with this we create an array of errors so that we can easily iterate over d errors in our frontend app
                    }
                                    }
                else
                {
                    // if error is not a string then it should be an object
                    foreach (var key in context.ModelState.Keys)
                    {
                        foreach (var error in context.ModelState[key].Errors)
                        {
                            response.Add($"{key}: {error.ErrorMessage}");
                        }

                    }
                }
                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
