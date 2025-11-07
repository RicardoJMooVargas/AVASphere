# Sistema de Inicialización y Verificación

## 📋 Descripción General

El sistema de inicialización verifica el estado de la aplicación al arrancar, determinando automáticamente si el usuario debe:
1. Configurar el sistema por primera vez (`/setup`)
2. Iniciar sesión (`/login`)
3. Acceder directamente a la aplicación (`/home`)

## 🔄 Flujo de Inicialización

```
┌─────────────────────────────────────────────────────────────┐
│                    INICIO DE APLICACIÓN                      │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │  ¿Primera vez?       │
              │  (Cache Check)       │
              └──────┬──────┬────────┘
                     │      │
            NO ◄─────┘      └─────► SÍ
             │                      │
             ▼                      ▼
    ┌──────────────────┐   ┌──────────────────┐
    │ Usar estado      │   │ Verificar con    │
    │ guardado en      │   │ servidor         │
    │ caché            │   │ (API Call)       │
    └────────┬─────────┘   └────────┬─────────┘
             │                      │
             │                      ▼
             │             ┌──────────────────┐
             │             │ Guardar estado   │
             │             │ en caché         │
             │             └────────┬─────────┘
             │                      │
             └──────────┬───────────┘
                        │
                        ▼
         ┌──────────────────────────────┐
         │ ¿Sistema Inicializado OK?    │
         └──────┬──────────────┬────────┘
                │              │
         NO ◄───┘              └───► SÍ
          │                          │
          ▼                          ▼
    ┌──────────┐         ┌──────────────────┐
    │ /setup   │         │ ¿Hay token?      │
    └──────────┘         └────┬────────┬────┘
                              │        │
                       NO ◄───┘        └───► SÍ
                        │                    │
                        ▼                    ▼
                  ┌──────────┐         ┌──────────┐
                  │ /login   │         │ /home    │
                  └──────────┘         └──────────┘
```

## 🏗️ Componentes del Sistema

### 1. **SystemInitService** (`system_init.service.dart`)

Servicio principal que coordina la verificación del sistema.

#### Métodos principales:

- **`determineInitialRoute()`**: Determina la ruta inicial basándose en el estado del sistema
- **`forceSystemCheck()`**: Fuerza una nueva verificación (útil después de completar setup)
- **`resetSystemState()`**: Resetea el estado de inicialización (testing/reinstalación)

### 2. **CacheService** (extendido)

Maneja el almacenamiento local de estado.

#### Nuevos métodos:

- **`isFirstTimeCheck()`**: Verifica si es la primera vez que se ejecuta la app
- **`markFirstTimeCheckDone()`**: Marca la primera verificación como completada
- **`saveSystemInitialized(bool)`**: Guarda el estado de inicialización del sistema
- **`isSystemInitialized()`**: Obtiene el estado guardado
- **`clearSystemInitFlags()`**: Limpia los flags de inicialización

### 3. **SystemService** (`system.service.dart`)

Conecta con el API para verificar el estado del sistema.

#### Endpoint usado:

- **`checkInitialConfig()`**: GET `/api/system/Config/check-initial-config`

### 4. **SplashScreen** (`splash_screen.dart`)

Pantalla de carga mostrada mientras se verifica el sistema (opcional).

## 📊 Modelo de Datos

### CheckInitConfigResponse

```dart
{
  "success": true,
  "message": "Success",
  "data": {
    "hasConfiguration": bool,      // ¿Tiene configuración inicial?
    "tableExists": bool,           // ¿Existe la tabla ConfigSys?
    "requiresMigration": bool,     // ¿Requiere migraciones?
    "message": string,             // Mensaje descriptivo
    "data": dynamic                // Datos adicionales (nullable)
  },
  "statusCode": 200,
  "timestamp": "2025-11-05T15:19:45.2905906Z"
}
```

### Lógica de Decisión

El sistema está **inicializado correctamente** cuando:
```dart
hasConfiguration == true && 
tableExists == true && 
requiresMigration == false
```

## 🔧 Configuración en main.dart

```dart
void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  
  // Determinar la ruta inicial basada en el estado del sistema
  final systemInitService = SystemInitService();
  final initialRoute = await systemInitService.determineInitialRoute();
  
  runApp(Principal(initialRoute: initialRoute));
}
```

## 📝 Casos de Uso

### Caso 1: Primera Instalación

1. Usuario abre la app por primera vez
2. `isFirstTimeCheck()` retorna `true`
3. Se llama al endpoint `/check-initial-config`
4. Si el sistema no está configurado → `/setup`
5. Se guarda el estado en caché
6. Se marca `firstTimeCheck` como completado

### Caso 2: App Ya Configurada

1. Usuario abre la app
2. `isFirstTimeCheck()` retorna `false`
3. Se lee el estado desde caché (sin llamar al servidor)
4. Sistema OK + sin token → `/login`
5. Sistema OK + con token → `/home`

### Caso 3: Sistema Requiere Setup

1. Verificación detecta problemas de configuración
2. Usuario es redirigido a `/setup`
3. Usuario completa la configuración
4. Se puede llamar a `forceSystemCheck()` para re-verificar
5. Si todo está OK, usuario va a `/login`

## 🎯 Ventajas del Sistema

1. **Eficiencia**: Solo verifica con el servidor la primera vez
2. **Velocidad**: Arranques subsecuentes son instantáneos
3. **Robustez**: Manejo de errores con fallback a rutas seguras
4. **Flexibilidad**: Fácil de resetear para testing
5. **Transparencia**: Logs detallados de cada paso

## 🔍 Debugging

### Ver logs en consola:

```
🚀 Iniciando verificación del sistema...
🔍 Primera verificación - consultando al servidor...
✅ Sistema inicializado correctamente sin token - Redirigiendo a login
```

### Resetear estado para testing:

```dart
final systemInitService = SystemInitService();
await systemInitService.resetSystemState();
```

### Forzar re-verificación:

```dart
final systemInitService = SystemInitService();
await systemInitService.forceSystemCheck();
```

## 🚀 Próximas Mejoras

- [ ] Agregar timeout para la verificación inicial
- [ ] Implementar caché con expiración (verificar cada X días)
- [ ] Agregar animación en SplashScreen
- [ ] Implementar retry logic en caso de error de red
- [ ] Agregar telemetría de inicialización

## 📚 Archivos Relacionados

- `lib/Core/services/system_init.service.dart`
- `lib/Core/services/data/cache.service.dart`
- `lib/Core/services/api/system.service.dart`
- `lib/Core/models/responses/check_init_config.module.dart`
- `lib/Core/screens/splash_screen.dart`
- `lib/main.dart`
- `lib/modules/login/screens/setup_page.dart`
