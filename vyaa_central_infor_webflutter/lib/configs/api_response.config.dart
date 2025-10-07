class ApiResponse<T> {
  final T? data;
  final String? error;

  ApiResponse.success(this.data) : error = null;
  ApiResponse.error(this.error) : data = null;

  bool get isSuccess => error == null;

  static String getErrorMessage(int statusCode, String responseBody) {
    switch (statusCode) {
      case 400:
        return 'Solicitud inválida: Verifique los datos enviados';
      case 401:
        return 'Credenciales inválidas: Usuario o contraseña incorrectos';
      case 403:
        return 'Acceso denegado: No tiene permisos para realizar esta acción';
      case 404:
        return 'Servicio no encontrado: El endpoint no existe';
      case 408:
        return 'Tiempo de espera agotado: Intente nuevamente';
      case 429:
        return 'Demasiadas solicitudes: Espere un momento antes de intentar nuevamente';
      case 500:
        return 'Error interno del servidor: Contacte al administrador';
      case 502:
        return 'Error de conexión: El servidor no está disponible';
      case 503:
        return 'Servicio no disponible: Intente más tarde';
      case 504:
        return 'Tiempo de espera del servidor agotado';
      default:
        return 'Error desconocido ($statusCode): $responseBody';
    }
  }
}
