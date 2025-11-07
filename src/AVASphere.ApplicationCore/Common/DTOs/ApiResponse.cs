using System.Text.Json.Serialization;

namespace AVASphere.ApplicationCore.Common.DTOs
{
    /// <summary>
    /// Respuesta estándar para todos los endpoints de la API
    /// </summary>
    /// <typeparam name="T">Tipo de datos que contiene la respuesta</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indica si la operación fue exitosa
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje descriptivo de la respuesta
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Datos de la respuesta (puede ser null)
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Código de estado HTTP
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Marca de tiempo de la respuesta
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Constructor para respuesta exitosa
        /// </summary>
        /// <param name="data">Datos de la respuesta</param>
        /// <param name="message">Mensaje opcional</param>
        /// <param name="statusCode">Código de estado HTTP</param>
        public ApiResponse(T data, string message = "Success", int statusCode = 200)
        {
            Success = true;
            Message = message;
            Data = data;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Constructor para respuesta de error
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        /// <param name="statusCode">Código de estado HTTP</param>
        /// <param name="data">Datos opcionales</param>
        public ApiResponse(string message, int statusCode, T? data = default)
        {
            Success = statusCode >= 200 && statusCode < 300;
            Message = message;
            Data = data;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Constructor sin parámetros para serialización
        /// </summary>
        public ApiResponse()
        {
        }
    }

    /// <summary>
    /// Respuesta estándar sin datos tipados
    /// </summary>
    public class ApiResponse : ApiResponse<object?>
    {
        public ApiResponse(string message, int statusCode, object? data = null) 
            : base(message, statusCode, data)
        {
        }

        public ApiResponse(object? data, string message = "Success", int statusCode = 200) 
            : base(data, message, statusCode)
        {
        }

        public ApiResponse() : base()
        {
        }
    }
}
