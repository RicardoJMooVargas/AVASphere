# Guía de Uso: Sistema de Inicialización

## 🚀 Inicio Rápido

El sistema de inicialización ahora funciona automáticamente. Simplemente ejecuta la aplicación:

```bash
flutter run
```

## 📱 Comportamiento de la Aplicación

### Primera Ejecución
1. La app verifica el estado del sistema con el servidor
2. **Si hay error de conexión** → Redirige a `/server-error` ⚠️
3. Si el sistema **NO está configurado** → Redirige a `/setup`
4. Si el sistema **está configurado** → Redirige a `/login`

### Ejecuciones Posteriores
1. La app usa el estado guardado en caché (sin llamar al servidor)
2. Redirige según el estado y si hay token de sesión

### 🔴 Manejo de Errores de Servidor
Si el servidor no está disponible, la aplicación muestra una pantalla de error dedicada con opciones para:
- Reintentar la conexión
- Ver configuración del servidor
- Continuar sin verificar (solo desarrollo)

Ver más detalles en [README_SERVER_ERROR_HANDLING.md](README_SERVER_ERROR_HANDLING.md)

## 🔧 Casos de Uso Comunes

### Caso 1: Testing - Forzar Ruta de Setup

Si necesitas probar la pantalla de setup:

```dart
// En cualquier parte de tu código
final systemInit = SystemInitService();
await systemInit.resetSystemState();
// Reinicia la aplicación
```

O desde la consola de Flutter:
```dart
// Presiona 'r' para hot reload después de ejecutar esto en debug
```

### Caso 2: Actualizar Estado Después de Configuración

Después de completar la configuración del sistema:

```dart
// En tu controlador de setup
final systemInit = SystemInitService();
await systemInit.forceSystemCheck();

// Determinar nueva ruta
final newRoute = await systemInit.determineInitialRoute();
Get.offAllNamed(newRoute); // Redirigir a la ruta apropiada
```

### Caso 3: Ver Diagnóstico del Sistema

Usa el widget de diagnóstico en tu SetupPage:

```dart
import 'package:vyaa_central_infor_webflutter/modules/login/widgets/system_diagnostic.widget.dart';

// En tu build method
const SystemDiagnosticWidget()
```

### Caso 4: Verificar Estado Manualmente

```dart
final systemService = SystemService();

// Verificar configuración
final configStatus = await systemService.checkInitialConfig();
print('Has Config: ${configStatus.data.hasConfiguration}');
print('Table Exists: ${configStatus.data.tableExists}');
print('Requires Migration: ${configStatus.data.requiresMigration}');

// Verificar migraciones
final migrations = await systemService.diagnoseMigrations();
print('Diagnosis: ${migrations.data.diagnosis}');
```

## 📂 Estructura de Archivos Importantes

```
lib/
├── main.dart                          # Punto de entrada - maneja inicialización
├── Core/
│   ├── services/
│   │   ├── system_init.service.dart   # Servicio de inicialización
│   │   ├── data/
│   │   │   └── cache.service.dart     # Servicio de caché (extendido)
│   │   └── api/
│   │       └── system.service.dart    # Servicio API del sistema
│   ├── models/
│   │   └── responses/
│   │       ├── check_init_config.module.dart      # Modelo de configuración
│   │       └── diagnose_migrations.module.dart    # Modelo de migraciones
│   └── screens/
│       └── splash_screen.dart         # Pantalla de carga (opcional)
└── modules/
    └── login/
        ├── screens/
        │   ├── setup_page.dart                      # Pantalla de setup básica
        │   └── setup_page_with_diagnostic.dart      # Setup con diagnóstico
        └── widgets/
            └── system_diagnostic.widget.dart        # Widget de diagnóstico
```

## 🧪 Testing

### Test 1: Primera Instalación

1. Elimina los datos de la app (o usa un nuevo emulador)
2. El servidor debe retornar `hasConfiguration: false`
3. Debe mostrar `/setup`

### Test 2: Sistema Configurado Sin Token

1. Sistema configurado correctamente
2. Sin token guardado
3. Debe mostrar `/login`

### Test 3: Sistema Configurado Con Token

1. Sistema configurado correctamente
2. Token válido guardado
3. Debe mostrar `/home`

### Test 4: Resetear Estado

```dart
// Método 1: Programáticamente
final systemInit = SystemInitService();
await systemInit.resetSystemState();

// Método 2: Limpiar caché completo
await CacheService.clearAll();
```

## 🐛 Debugging

### Activar Logs Detallados

Los logs ya están incluidos. Busca en la consola:

```
🚀 Iniciando verificación del sistema...
🔍 Primera verificación - consultando al servidor...
✅ Sistema inicializado correctamente sin token - Redirigiendo a login
```

### Problemas Comunes

#### Problema: La app siempre va a /setup
**Solución**: Verifica que el servidor retorne los valores correctos:
```json
{
  "data": {
    "hasConfiguration": true,
    "tableExists": true,
    "requiresMigration": false
  }
}
```

#### Problema: La app no llama al servidor
**Solución**: Resetea el estado para forzar verificación:
```dart
await CacheService.clearSystemInitFlags();
```

#### Problema: Error al conectar con el servidor
**Solución**: Verifica la configuración del baseUrl en `api_settings.config.dart`

## 📊 Estados del Sistema

| hasConfiguration | tableExists | requiresMigration | Resultado |
|------------------|-------------|-------------------|-----------|
| ✅ true         | ✅ true    | ❌ false         | ✅ Sistema OK → Login/Home |
| ❌ false        | -          | -                 | ⚠️ Requiere Setup |
| -               | ❌ false   | -                 | ⚠️ Requiere Setup |
| -               | -          | ✅ true          | ⚠️ Requiere Setup |

## 🎯 Próximos Pasos

1. **Implementar formulario de configuración** en SetupPage
2. **Agregar validaciones** antes de guardar configuración
3. **Implementar timeout** para la verificación inicial
4. **Agregar tests unitarios** para SystemInitService
5. **Crear página de diagnóstico** accesible desde configuración

## 📚 Referencias

- [README_SYSTEM_INITIALIZATION.md](README_SYSTEM_INITIALIZATION.md) - Documentación técnica completa
- [README_API_MAPPING.md](README_API_MAPPING.md) - Documentación de endpoints
- [api_endpoints.config.dart](lib/configs/api_endpoints.config.dart) - Configuración de endpoints

## 💡 Consejos

- **Producción**: El sistema solo verifica con el servidor la primera vez, mejorando el rendimiento
- **Desarrollo**: Usa `resetSystemState()` para probar diferentes escenarios
- **Testing**: Usa el widget de diagnóstico para ver el estado en tiempo real
- **Debug**: Los logs en consola te ayudarán a entender el flujo de inicialización
