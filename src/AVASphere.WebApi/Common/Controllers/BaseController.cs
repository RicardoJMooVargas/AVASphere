using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.WebApi.Common.Extensions;

namespace AVASphere.WebApi.Common.Controllers
{
    /// <summary>
    /// Controlador base que proporciona funcionalidad común para todos los controladores de la API
    /// </summary>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected BaseController(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Crea una respuesta exitosa con datos
        /// </summary>
        /// <typeparam name="T">Tipo de datos</typeparam>
        /// <param name="data">Datos a devolver</param>
        /// <param name="message">Mensaje personalizado</param>
        /// <returns>Respuesta estándar con datos</returns>
        protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "Operation completed successfully")
        {
            return this.OkResponse(data, message);
        }

        /// <summary>
        /// Crea una respuesta exitosa sin datos específicos
        /// </summary>
        /// <param name="message">Mensaje de éxito</param>
        /// <returns>Respuesta estándar sin datos</returns>
        protected ActionResult<ApiResponse> Success(string message = "Operation completed successfully")
        {
            return this.OkResponse(null, message);
        }

        /// <summary>
        /// Crea una respuesta de error de validación
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        /// <param name="validationErrors">Errores de validación específicos</param>
        /// <returns>Respuesta de error 400</returns>
        protected ActionResult<ApiResponse> ValidationError(string message = "Validation failed", object? validationErrors = null)
        {
            return this.BadRequestResponse(message, validationErrors);
        }

        /// <summary>
        /// Crea una respuesta de recurso no encontrado
        /// </summary>
        /// <param name="resource">Nombre del recurso</param>
        /// <param name="identifier">Identificador del recurso</param>
        /// <returns>Respuesta de error 404</returns>
        protected ActionResult<ApiResponse> NotFound(string resource, object identifier)
        {
            return this.NotFoundResponse($"{resource} with identifier '{identifier}' not found");
        }

        /// <summary>
        /// Crea una respuesta de conflicto (recurso ya existe)
        /// </summary>
        /// <param name="message">Mensaje de conflicto</param>
        /// <returns>Respuesta de error 409</returns>
        protected ActionResult<ApiResponse> Conflict(string message)
        {
            return this.ConflictResponse(message);
        }

        /// <summary>
        /// Crea una respuesta de recurso creado
        /// </summary>
        /// <typeparam name="T">Tipo de datos creados</typeparam>
        /// <param name="data">Datos del recurso creado</param>
        /// <param name="actionName">Nombre de la acción para obtener el recurso</param>
        /// <param name="routeValues">Valores de ruta</param>
        /// <param name="message">Mensaje personalizado</param>
        /// <returns>Respuesta 201 Created</returns>
        protected ActionResult<ApiResponse<T>> Created<T>(T data, string actionName, object routeValues, string message = "Resource created successfully")
        {
            return this.CreatedResponse(data, actionName, routeValues, message);
        }

        /// <summary>
        /// Maneja excepciones comunes y devuelve respuestas apropiadas
        /// </summary>
        /// <param name="ex">Excepción a manejar</param>
        /// <param name="operationName">Nombre de la operación que falló</param>
        /// <returns>Respuesta de error apropiada</returns>
        protected ActionResult<ApiResponse> HandleException(Exception ex, string operationName)
        {
            _logger.LogError(ex, "Error in operation: {OperationName}", operationName);

            return ex switch
            {
                KeyNotFoundException => this.NotFoundResponse(ex.Message),
                ArgumentException => this.BadRequestResponse(ex.Message),
                InvalidOperationException when ex.Message.Contains("ya está en uso") => this.ConflictResponse(ex.Message),
                InvalidOperationException => this.BadRequestResponse(ex.Message),
                UnauthorizedAccessException => this.UnauthorizedResponse(ex.Message),
                _ => this.InternalServerErrorResponse("An unexpected error occurred")
            };
        }

        /// <summary>
        /// Valida el ModelState y devuelve errores si no es válido
        /// </summary>
        /// <returns>Respuesta de error si el ModelState no es válido, null si es válido</returns>
        protected ActionResult<ApiResponse>? ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return ValidationError("Model validation failed", errors);
            }

            return null;
        }
    }
}
