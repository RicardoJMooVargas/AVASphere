using AVASphere.ApplicationCore.Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AVASphere.WebApi.Common.Filters
{
    /// <summary>
    /// Filtro de acción que valida automáticamente el ModelState y devuelve respuestas estándar
    /// </summary>
    public class ValidateModelStateFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var response = new ApiResponse("Validation failed", 400, errors);
                context.Result = new BadRequestObjectResult(response);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Filtro de resultado que envuelve respuestas no estándar en el formato ApiResponse
    /// </summary>
    public class StandardizeResponseFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // Solo procesar respuestas exitosas que no sean ya ApiResponse
            if (context.Result is ObjectResult objectResult && 
                objectResult.StatusCode >= 200 && 
                objectResult.StatusCode < 300 &&
                objectResult.Value != null &&
                !IsApiResponseType(objectResult.Value.GetType()))
            {
                var response = new ApiResponse(objectResult.Value, "Success", objectResult.StatusCode ?? 200);
                objectResult.Value = response;
            }

            base.OnActionExecuted(context);
        }

        private static bool IsApiResponseType(Type type)
        {
            // Verificar si el tipo es ApiResponse o ApiResponse<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>))
                return true;
            
            return type == typeof(ApiResponse);
        }
    }
}
