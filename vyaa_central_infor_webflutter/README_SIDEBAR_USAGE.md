// Ejemplo de uso avanzado del SidebarLayout

// 1. Página con sidebar por defecto
GetPage(
  name: '/home',
  page: () => const SidebarLayout(
    child: HomePage(),
  ),
),

// 2. Página con sidebar personalizado
GetPage(
  name: '/admin',
  page: () => SidebarLayout(
    userAvatarTooltip: 'Administrador',
    customSidebarItems: [
      SidebarItem(
        name: 'Usuarios',
        icon: Icons.people,
        onPress: () => Get.toNamed('/admin/users'),
      ),
      SidebarItem(
        name: 'Configuración',
        icon: Icons.settings,
        onPress: () => Get.toNamed('/admin/settings'),
      ),
    ],
    child: const AdminPage(),
  ),
),

// 3. Página sin sidebar
GetPage(
  name: '/login',
  page: () => const NoSidebarLayout(
    child: LoginPage(),
  ),
),

// 4. Página con sidebar pero sin logout
GetPage(
  name: '/readonly',
  page: () => const SidebarLayout(
    showLogout: false,
    child: ReadOnlyPage(),
  ),
),