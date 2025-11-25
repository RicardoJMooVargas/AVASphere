# AVASphere
Multiples herramientas al uso para la empresa.

## Getting Started

- Modificaciones relacionadas a la carga del sistema en [system_init_screen.dart](lib/Core/screens/system_init_screen.dart)

## 🔐 Manejo de Datos de Usuario y Permisos

### Acceso a Módulos y Permisos

Después del login, los módulos y permisos del usuario se guardan en Hive y pueden ser accedidos mediante `UserDataService`:

```dart
import 'package:vyaa_central_infor_webflutter/Core/core.dart';

// Obtener módulos del usuario
final modules = await UserDataService.getUserModules();
for (var module in modules) {
  print('${module.name} → ${module.normalized}');
}

// Verificar acceso a un módulo (acepta nombre o ruta)
final hasAccess = await UserDataService.hasModuleAccess('Sales');
// O también: await UserDataService.hasModuleAccess('/sales');
if (hasAccess) {
  // Mostrar funcionalidad de ventas
}

// Verificar acceso a múltiples módulos
final access = await UserDataService.hasMultipleModuleAccess(['Sales', 'Inventory', 'Projects']);
print('Sales: ${access['Sales']}');
print('Inventory: ${access['Inventory']}');
print('Projects: ${access['Projects']}');

// Verificar permiso específico
final canEdit = await UserDataService.hasPermission('FULL_ACCESS');

// Verificar si es super admin
final isSuperAdmin = await UserDataService.isSuperAdmin();

// Obtener información completa
final user = await UserDataService.getFullUserData();
print('Usuario: ${user?.name} ${user?.lastName}');
print('Módulos: ${user?.modules.length}');
print('Permisos: ${user?.permissions.length}');

// Log de información del usuario (debugging)
await UserDataService.logUserInfo();
```

### 🎨 Sidebar Dinámico Basado en Permisos

El `MainAppLayout` filtra automáticamente los items del sidebar según los módulos del usuario:

**Configuración en `routes.config.dart`:**
```dart
static const List<SidebarItemData> sidebarItems = [
  SidebarItemData(
    title: 'Ventas',
    icon: Icons.sell_outlined,
    route: '/sales',
    order: 2,
    moduleName: 'Sales', // ← Debe coincidir con el módulo del backend
  ),
  // ... más items
];
```

**Flujo de Filtrado:**
1. Usuario inicia sesión → Módulos guardados en Hive
2. `MainAppLayout` carga → Lee módulos del usuario
3. Filtra items del sidebar → Solo muestra los permitidos
4. **Excepción:** `/home` y `/dashboard` siempre se muestran

**Logs de Debug:**
```
🔐 ========== CARGANDO PERMISOS DEL USUARIO ==========
📦 Módulos del usuario: 7
   - General (/general)
   - Sales (/sales)
   - Inventory (/inventory)
✅ Permitiendo: Dashboard (ruta base)
✅ Permitiendo: Ventas → /sales [Sales]
✅ Permitiendo: Inventario → /inventory [Inventory]
🚫 Bloqueando: Sistema → /system [System]
📊 Sidebar filtrado: 3 de 6 items
🔐 ========== FIN CARGA DE PERMISOS ==========
```

### Estructura de Datos

**Módulos del Usuario:**
```json
{
  "index": 0,
  "name": "Sales",
  "normalized": "/sales"
}
```

**Permisos del Usuario:**
```json
{
  "index": 0,
  "name": "Acceso Total",
  "normalized": "FULL_ACCESS",
  "type": "SUPER_ADMIN",
  "status": "active"
}
```



## FRONTEND COMANDOS

Ejecuta estos comandos desde la carpeta raíz del proyecto Flutter.

```bash
# Limpia la build y archivos temporales
flutter clean

# Actualiza las dependencias a las últimas versiones mayores disponibles.
# Atención: puede introducir cambios incompatibles (breaking changes).
flutter pub upgrade --major-versions

# Descarga las dependencias especificadas en pubspec.yaml
flutter pub get
```

Notas:
- Ejecuta los comandos en el terminal desde la raíz del proyecto.
- Haz un commit o crea una rama antes de `flutter pub upgrade --major-versions` si quieres poder revertir cambios en dependencias.

