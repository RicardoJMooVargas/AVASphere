# Manejo de Errores de Servidor

## 🔴 Pantalla de Error de Servidor

Cuando la aplicación no puede conectarse con el servidor, ahora muestra una pantalla dedicada de error en lugar de simplemente redirigir al login.

## 🎯 Casos de Error Detectados

El sistema detecta automáticamente los siguientes errores de conexión:

- ❌ `ClientException: Failed to fetch`
- ❌ `SocketException`
- ❌ `Connection refused/timeout`
- ❌ Errores de red generales

## 📱 Pantalla de Error

### Ubicación
`lib/Core/screens/server_error_screen.dart`

### Características

1. **Diseño Visual Atractivo**
   - Gradiente rojo para indicar error
   - Icono de nube desconectada
   - Mensaje claro y descriptivo

2. **Información Útil**
   - Lista de verificación para el usuario
   - URL del servidor
   - Endpoint que falló

3. **Acciones Disponibles**
   - 🔄 **Reintentar Conexión**: Vuelve a verificar el servidor
   - ⚙️ **Ver Configuración**: Muestra la URL y endpoint actual
   - ➡️ **Continuar sin verificar**: Solo para desarrollo

## 🔄 Flujo de Error

```
INICIO
  ↓
Verificar Sistema
  ↓
¿Error de Conexión?
  ├─ SÍ → /server-error
  │         ↓
  │     Usuario presiona "Reintentar"
  │         ↓
  │     ¿Conexión OK?
  │       ├─ SÍ → Ruta apropiada (/setup, /login, /home)
  │       └─ NO → Mensaje de error + mantener en /server-error
  │
  └─ NO → Continuar flujo normal
```

## 🧪 Testing

### Simular Error de Servidor

1. **Detener el servidor backend**
2. **Iniciar la aplicación**
3. **Resultado esperado**: Pantalla de error de servidor

### Probar Reintento

1. **Estar en pantalla de error**
2. **Iniciar el servidor**
3. **Presionar "Reintentar Conexión"**
4. **Resultado esperado**: Redirige a la pantalla apropiada

## 💻 Código de Detección

```dart
// En SystemInitService.determineInitialRoute()
catch (e) {
  // Verificar si es un error de conexión al servidor
  final errorString = e.toString().toLowerCase();
  final isConnectionError = errorString.contains('clientexception') ||
                            errorString.contains('failed to fetch') ||
                            errorString.contains('connection') ||
                            errorString.contains('network') ||
                            errorString.contains('socketexception') ||
                            errorString.contains('timeout');
  
  if (isConnectionError) {
    debugPrint('🔴 Error de conexión detectado - Redirigiendo a pantalla de error de servidor');
    return '/server-error';
  }
  
  // Otros errores...
}
```

## 📊 Mensajes de Log

### Error Detectado
```
❌ Error al verificar configuración inicial: Exception: Error inesperado: ClientException: Failed to fetch
❌ Error al verificar estado del sistema: Exception: Error inesperado: ClientException: Failed to fetch
🔴 Error de conexión detectado - Redirigiendo a pantalla de error de servidor
```

### Reintento Exitoso
```
🔍 Verificando configuración inicial del sistema...
✅ Sistema inicializado correctamente sin token - Redirigiendo a login
```

## 🎨 Personalización

### Cambiar URL Mostrada

Edita `ServerErrorScreen` para obtener la URL desde la configuración:

```dart
// Importar configuración
import '../../../configs/api_settings.config.dart';

// Usar en el diálogo
SelectableText(
  ApiSettings().baseUrl,
  style: TextStyle(fontFamily: 'monospace'),
),
```

### Agregar Más Información

Puedes mostrar información adicional del error:

```dart
// Pasar el error como parámetro
GetPage(
  name: '/server-error',
  page: () => ServerErrorScreen(
    error: Get.parameters['error'],
  ),
),
```

## 🛠️ Solución de Problemas

### Problema: No detecta el error
**Causa**: El error no está en la lista de detección
**Solución**: Agregar el patrón del error en `isConnectionError`

### Problema: Reintento no funciona
**Causa**: El servidor aún no está disponible
**Solución**: Verificar que el servidor esté corriendo y accesible

### Problema: Botón "Continuar sin verificar" no debería estar visible
**Causa**: Es para desarrollo
**Solución**: Eliminar ese botón en producción o condicionarlo:

```dart
if (kDebugMode)
  TextButton.icon(
    // Código del botón
  ),
```

## 📋 Checklist de Verificación

Antes de reportar un problema de conexión, verifica:

- [ ] ¿El servidor está ejecutándose?
- [ ] ¿La URL en `api_settings.config.dart` es correcta?
- [ ] ¿Puedes acceder a la URL desde el navegador?
- [ ] ¿El firewall está bloqueando la conexión?
- [ ] ¿Estás usando HTTPS con certificado válido?
- [ ] ¿El servidor acepta conexiones desde tu IP?

## 🔐 Consideraciones de Seguridad

### En Producción

1. **Remover botón "Continuar sin verificar"**
2. **No mostrar URLs completas en pantalla de error**
3. **Agregar rate limiting en reintentos**
4. **Log de errores en servidor de telemetría**

### Ejemplo de Implementación Segura

```dart
// Solo mostrar en desarrollo
if (kDebugMode)
  TextButton.icon(
    onPressed: () => Get.offAllNamed('/login'),
    label: const Text('Continuar sin verificar'),
  ),

// No exponer URL completa
const Text('No se pudo conectar con el servidor')
// En lugar de mostrar la URL completa
```

## 🚀 Próximas Mejoras

- [ ] Agregar timeout configurable para reintentos
- [ ] Implementar exponential backoff para reintentos automáticos
- [ ] Mostrar estado de conexión en tiempo real
- [ ] Agregar opción para cambiar URL del servidor (modo desarrollo)
- [ ] Implementar sistema de telemetría de errores
- [ ] Agregar modo offline con funcionalidad limitada

## 📚 Archivos Relacionados

- `lib/Core/screens/server_error_screen.dart` - Pantalla de error
- `lib/Core/services/system_init.service.dart` - Detección de error
- `lib/main.dart` - Ruta `/server-error`
- `lib/configs/api_settings.config.dart` - Configuración del servidor
