# Migración a GoRouter

## 📋 Resumen

Se ha implementado **go_router** como el sistema de navegación principal de la aplicación, reemplazando GetX. El sistema mantiene la configuración centralizada de rutas en `routes.config.dart` y usa el servicio `route_app.service.dart` para traducir esta configuración a GoRouter.

## 🏗️ Arquitectura

### Componentes Principales

1. **routes.config.dart** 
   - Define todas las rutas usando el modelo `RouteConfig`
   - Mantiene la jerarquía de rutas (principales y subrutas)
   - Configura permisos, módulos y visualización en sidebar

2. **route_app.service.dart**
   - Traduce `RouteConfig` a `GoRoute` de go_router
   - Maneja la ruta especial `/app` que envuelve las pantallas con `MainAppLayout`
   - Gestiona redirecciones globales
   - Filtra rutas según permisos del usuario

3. **main.dart**
   - Inicializa la aplicación
   - Determina la ruta inicial basándose en el estado del sistema
   - Crea el `GoRouter` y lo pasa a `MaterialApp.router`

## 🚀 Uso

### Navegación Básica

```dart
import 'package:go_router/go_router.dart';

// Navegar a una ruta
context.go('/home');
context.go('/sales');

// Navegar con parámetros
context.go('/sales/12345');

// Navegar reemplazando la ruta actual
context.replace('/login');

// Volver atrás
context.pop();
```

### Extensiones Personalizadas

El servicio incluye extensiones para navegación simplificada:

```dart
// Navegar a una ruta de app
context.toAppRoute('/sales');

// Ir al home
context.offAllToHome();

// Ir al login
context.offAllToLogin();

// Ir al setup
context.offAllToSetup();

// Ir al error del servidor
context.toServerError();
```

### Usando RouteAppService Directamente

```dart
import 'package:vyaa_central_infor_webflutter/Core/services/data/route_app.service.dart';

// Navegar
RouteAppService.navigateTo(context, '/home');

// Obtener rutas del sidebar filtradas
final sidebarRoutes = RouteAppService.getSidebarRoutes(
  userModules: ['Sales', 'Inventory'],
);
```

## 📝 Agregar Nueva Ruta

### 1. Ruta Simple (Sin Subrutas)

En `routes.config.dart`:

```dart
RouteConfig(
  name: 'reports',
  path: '/reports',
  page: () => const ReportsPage(),
  title: 'Reportes',
  icon: Icons.analytics_outlined,
  isFullScreen: false,        // Con sidebar
  requiresAuth: true,          // Requiere login
  showInSidebar: true,         // Aparece en menú
  sidebarOrder: 7,             // Orden en el menú
  moduleName: 'Reports',       // Módulo del backend
),
```

### 2. Ruta con Subrutas

```dart
RouteConfig(
  name: 'customers',
  path: '/customers',
  page: () => const CustomersListPage(),
  title: 'Clientes',
  icon: Icons.people_outlined,
  isFullScreen: false,
  requiresAuth: true,
  showInSidebar: true,
  sidebarOrder: 8,
  moduleName: 'Customers',
  subRoutes: [
    RouteConfig(
      name: 'customer_detail',
      path: '/customers/:customerId',
      page: () => const CustomerDetailPage(),
      title: 'Detalle de Cliente',
      isSubRoute: true,
      parentPath: '/customers',
      requiresAuth: true,
      moduleName: 'Customers',
    ),
  ],
),
```

### 3. Ruta de Pantalla Completa (Sin Sidebar)

```dart
RouteConfig(
  name: 'report_preview',
  path: '/report/:reportId/preview',
  page: () => const ReportPreviewPage(),
  title: 'Vista Previa de Reporte',
  isFullScreen: true,         // Pantalla completa
  requiresAuth: true,
  showInSidebar: false,        // No en menú
  moduleName: 'Reports',
),
```

## 🔄 Flujo de Rutas

### Rutas del Sistema

Estas rutas se muestran en pantalla completa sin sidebar:

- `/` - Inicialización del sistema
- `/login` - Inicio de sesión
- `/setup` - Configuración inicial
- `/server-error` - Error del servidor
- `/404` - Página no encontrada

### Rutas de la Aplicación

Estas rutas se envuelven automáticamente en `MainAppLayout`:

- `/home` → `/app?route=/home`
- `/sales` → `/app?route=/sales`
- `/inventory` → `/app?route=/inventory`
- etc.

El servicio detecta automáticamente qué rutas necesitan el layout y las redirige apropiadamente.

## 🎯 Ventajas del Sistema

### ✅ Configuración Centralizada
- Todas las rutas definidas en un solo archivo
- Fácil de mantener y actualizar

### ✅ Filtrado Automático por Permisos
- Las rutas se filtran según los módulos del usuario
- Seguridad integrada en el sistema de navegación

### ✅ Manejo Automático de Layout
- Las rutas con `requiresAuth` y sin `isFullScreen` se envuelven automáticamente en `MainAppLayout`
- No necesitas especificar el layout en cada ruta

### ✅ Jerarquía Clara
- Soporte para rutas anidadas a múltiples niveles
- Relación clara entre rutas principales y subrutas

### ✅ Integración con Sidebar
- Control granular de qué rutas aparecen en el menú
- Ordenamiento flexible

## ⚠️ Consideraciones Importantes

1. **MainAppLayout**: Las rutas internas se gestionan dentro del layout. La navegación fuera del layout usa go_router normal.

2. **Parámetros de Ruta**: Usa `:paramName` en el path para definir parámetros:
   ```dart
   path: '/sales/:saleId'
   ```

3. **Query Parameters**: Se usan para pasar la ruta interna a `/app`:
   ```dart
   '/app?route=/home'
   ```

4. **GetX Removido**: La migración elimina la dependencia de GetX para navegación. Si usas GetX para otros propósitos (state management), asegúrate de mantener esas dependencias.

## 🧪 Testing

Para probar el sistema de rutas:

```bash
flutter run -d chrome
```

Verifica:
- ✅ La inicialización carga correctamente
- ✅ El login funciona y redirige al home
- ✅ El sidebar muestra las rutas correctas
- ✅ La navegación entre páginas funciona
- ✅ Las subrutas se cargan correctamente
- ✅ La ruta 404 aparece para URLs inválidas

## 📚 Referencias

- [go_router Package](https://pub.dev/packages/go_router)
- [Flutter Navigation and Routing](https://docs.flutter.dev/ui/navigation)
- [GoRouter Documentation](https://gorouter.dev/)
