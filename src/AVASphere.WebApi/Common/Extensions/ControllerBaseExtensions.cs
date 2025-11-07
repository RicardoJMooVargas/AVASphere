using AVASphere.ApplicationCore.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Common.Extensions
{
    /// <summary>
    /// Extensiones para ControllerBase para crear respuestas estándar
    /// </summary>
    public static class ControllerBaseExtensions
    {
        /// <summary>
        /// Crea una respuesta exitosa estándar
        /// </summary>
        /// <typeparam name="T">Tipo de datos</typeparam>
        /// <param name="controller">Controlador</param>
        /// <param name="data">Datos de respuesta</param>
        /// <param name="message">Mensaje personalizado</param>
        /// <returns>ActionResult con respuesta estándar</returns>
        public static ActionResult<ApiResponse<T>> OkResponse<T>(this ControllerBase controller, T data, string message = "Success")
        {
            var response = new ApiResponse<T>(data, message, 200);
            return controller.Ok(response);
        }

        /// <summary>
        /// Crea respuesta exitosa sin datos tipados
        /// </summary>
        /// <param name="controller">Controlador</param>
        /// <param name="data">Datos de respuesta</param>
        /// <param name="message">Mensaje personalizado</param>
        /// <returns>ActionResult con respuesta estándar</returns>
        public static ActionResult<ApiResponse> OkResponse(this ControllerBase controller, object? data = null, string message = "Success")
        {
            var response = new ApiResponse(data, message, 200);
            return controller.Ok(response);
        }

        /// <summary>
        /// Crea una respuesta de recurso creado
        /// </summary>
        /// <typeparam name="T">Tipo de datos</typeparam>
        /// <param name="controller">Controlador</param>
        /// <param name="data">Datos creados</param>
        /// <param name="actionName">Nombre de la acción</param>
        /// <param name="routeValues">Valores de ruta</param>
        /// <param name="message">Mensaje personalizado</param>
        /// <returns>ActionResult con respuesta estándar</returns>
        public static ActionResult<ApiResponse<T>> CreatedResponse<T>(this ControllerBase controller, T data, string actionName, object routeValues, string message = "Resource created successfully")
        {
            var response = new ApiResponse<T>(data, message, 201);
            return controller.CreatedAtAction(actionName, routeValues, response);
        }

        /// <summary>
        /// Crea una respuesta de recurso no encontrado
        /// </summary>
        /// <param name="controller">Controlador</param>
        /// <param name="message">Mensaje de error</param>
        /// <returns>ActionResult con respuesta estándar</returns>
        public static ActionResult<ApiResponse> NotFoundResponse(this ControllerBase controller, string message = "Resource not found")
        {
            var response = new ApiResponse(message, 404);
            return controller.NotFound(response);
        }

        /// <summary>
        /// Crea una respuesta de solicitud incorrecta
        /// </summary>
        /// <param name="controller">Controlador</param>
        /// <param name="message">Mensaje de error</param>
        /// <param name="data">Datos adicionales del error</param>
        /// <returns>ActionResult con respuesta estándar</returns>
        public static ActionResult<ApiResponse> BadRequestResponse(this ControllerBase controller, string message = "Bad request", object? data = null)
        {
            var response = new ApiResponse(message, 400, data);
            return controller.BadRequest(response);
        }

        /// <summary>
        /// Crea una respuesta de conflicto
        /// </summary>
        /// <param name="controller">Controlador</param>
        /// <param name="message">Mensaje de error</param>
        /// <returns>ActionResult con respuesta estándar</returns>
        public static ActionResult<ApiResponse> ConflictResponse(this ControllerBase controller, string message = "Conflict")
        {
            var response = new ApiResponse(message, 409);
            return controller.Conflict(response);
        }

        /// <summary>
        /// Crea una respuesta de no autorizado
        /// </summary>
        /// <param name="controller">Controlador</param>
        /// <param name="message">Mensaje de error</param>
        /// <returns>ActionResult con respuesta estándar</returns>
        public static ActionResult<ApiResponse> UnauthorizedResponse(this ControllerBase controller, string message = "Unauthorized")
        {
            var response = new ApiResponse(message, 401);
            return controller.Unauthorized(response);
        }

        /// <summary>
        /// Crea una respuesta de error interno del servidor
        /// </summary>
        /// <param name="controller">Controlador</param>
        /// <param name="message">Mensaje de error</param>
        /// <param name="error">Información adicional del error</param>
        /// <returns>ActionResult con respuesta estándar</returns>
        public static ActionResult<ApiResponse> InternalServerErrorResponse(this ControllerBase controller, string message = "Internal server error", object? error = null)
        {
            var response = new ApiResponse(message, 500, error);
            return controller.StatusCode(500, response);
        }

        /// <summary>
        /// Crea una respuesta sin contenido
        /// </summary>
        /// <param name="controller">Controlador</param>
        /// <param name="message">Mensaje personalizado</param>
        /// <returns>ActionResult con respuesta estándar</returns>
        public static ActionResult<ApiResponse> NoContentResponse(this ControllerBase controller, string message = "Operation completed successfully")
        {
            var response = new ApiResponse(null, message, 204);
            return controller.StatusCode(204, response);
        }
    }
}
