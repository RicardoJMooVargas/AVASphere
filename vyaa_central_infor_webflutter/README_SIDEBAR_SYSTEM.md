# Sistema de Layouts con Sidebar

## Descripción
Este sistema proporciona una forma eficiente y flexible de manejar el sidebar en toda la aplicación Flutter usando GetX.

## Estructura

### Componentes Principales:

1. **SidebarLayout**: Wrapper que incluye el sidebar
2. **NoSidebarLayout**: Wrapper para páginas sin sidebar
3. **AppSidebar**: Componente base del sidebar (ya existía)

## Uso en Rutas

### Páginas con Sidebar (Rutas principales)
```dart
GetPage(
  name: '/home',
  page: () => const SidebarLayout(
    userAvatarTooltip: 'Usuario Admin',
    child: HomePage(),
  ),
),
```

### Páginas sin Sidebar (Login, etc.)
```dart
GetPage(
  name: '/login',
  page: () => const NoSidebarLayout(
    child: LoginPage(),
  ),
),
```

### Páginas con Sidebar Personalizado
```dart
GetPage(
  name: '/admin',
  page: () => SidebarLayout(
    customSidebarItems: [
      SidebarItem(
        name: 'Configuración',
        icon: Icons.settings,
        onPress: () => Get.toNamed('/admin/settings'),
      ),
    ],
    child: const AdminPage(),
  ),
),
```

## Ventajas del Sistema

### ✅ Eficiencia
- El sidebar se configura una vez por ruta
- No se duplica código en cada página
- Fácil mantenimiento centralizado

### ✅ Flexibilidad
- Rutas pueden tener sidebar o no
- Sidebar personalizable por ruta
- Control granular de funcionalidades

### ✅ Consistencia
- Navegación uniforme en toda la app
- Items del sidebar centralizados
- Comportamiento predecible

### ✅ Mantenibilidad
- Cambios en el sidebar se aplican globalmente
- Fácil agregar nuevas rutas con/sin sidebar
- Configuración declarativa y clara

## Estructura de Archivos

```
lib/
  Core/
    layouts/
      sidebar_layout.dart          # Layouts principales
      README_SIDEBAR_USAGE.md      # Esta documentación
    Widgets/
      app_sidebar.widget.dart      # Componente base del sidebar
  modules/
    dashboard/
      screens/
        home_page.dart            # Solo el contenido, sin sidebar
    sales/
      screens/
        sales_page.dart           # Solo el contenido, sin sidebar
    login/
      screens/
        login_page.dart           # Sin sidebar
  main.dart                       # Configuración de rutas
```

## Navegación

El sidebar utiliza `Get.offNamed()` para evitar el stack de navegación:

```dart
SidebarItem(
  name: 'Dashboard',
  icon: Icons.dashboard,
  onPress: () => Get.offNamed('/home'), // Reemplaza la ruta actual
),
```

## Implementación del Home

### HomePage con "En Desarrollo"
- ✅ Icono de herramientas (🔧)
- ✅ Mensaje "Dashboard en Desarrollo" 
- ✅ Card informativa con módulos disponibles
- ✅ Diseño responsive y atractivo
- ✅ AppBar sin botón de atrás (automaticallyImplyLeading: false)

### Páginas de Módulos
Cada módulo tiene su propia página placeholder:
- `/sales` - Módulo de Ventas
- `/inventory` - Módulo de Inventario  
- `/supply` - Módulo de Suministros

## Configuración por Defecto

Los items del sidebar por defecto son:
- 🏠 Dashboard (`/home`)
- 💼 Ventas (`/sales`)
- 📦 Inventario (`/inventory`)
- 🚚 Suministros (`/supply`)

## Customización Futura

Para agregar nuevas funcionalidades:

1. **Nueva ruta con sidebar**:
```dart
GetPage(
  name: '/nueva-ruta',
  page: () => const SidebarLayout(
    child: NuevaPagina(),
  ),
),
```

2. **Ruta sin sidebar**:
```dart
GetPage(
  name: '/modal',
  page: () => const NoSidebarLayout(
    child: ModalPage(),
  ),
),
```

3. **Sidebar especial**:
```dart
GetPage(
  name: '/admin',
  page: () => SidebarLayout(
    customSidebarItems: [...],
    showLogout: false,
    child: const AdminPage(),
  ),
),
```

## Estado Actual

✅ **Implementado**:
- Sistema de layouts funcional
- Home page con diseño "en desarrollo"
- Navegación entre módulos
- Páginas placeholder para cada módulo
- Configuración eficiente en main.dart

🔄 **En Desarrollo**:
- Funcionalidades específicas de cada módulo
- Integración con APIs
- Contenido real de las páginas