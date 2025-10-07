import 'package:flutter/material.dart';
import '../theme/app_colors.dart';
import '../internalControllers/notification_services.dart';

class AppSidebar extends StatefulWidget {
  final Widget body;
  final List<SidebarItem> sidebarItems;
  final String? userAvatarTooltip;
  final bool showLogout;
  
  const AppSidebar({
    Key? key,
    required this.body,
    required this.sidebarItems,
    this.userAvatarTooltip = 'Usuario',
    this.showLogout = true,
  }) : super(key: key);

  @override
  State<AppSidebar> createState() => _AppSidebarState();
}

class _AppSidebarState extends State<AppSidebar> {
  int _selectedIndex = 0;

  void _logout() {
    NotificationService.showLogoutConfirmation(
      onConfirm: () => NotificationService.handleLogout(),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Row(
        children: [
          // Barra lateral
          Container(
            width: 60,
            height: double.infinity,
            decoration: const BoxDecoration(
              color: AppColors.tertiaryColor,
              boxShadow: [
                BoxShadow(
                  color: Colors.black12,
                  blurRadius: 10,
                  offset: Offset(2, 0),
                ),
              ],
            ),
            child: Column(
              children: [
                const SizedBox(height: 20),
                
                // Avatar del usuario
                Tooltip(
                  message: widget.userAvatarTooltip ?? 'Usuario',
                  child: CircleAvatar(
                    radius: 20,
                    backgroundColor: AppColors.primaryColor,
                    child: const Icon(
                      Icons.person,
                      color: Colors.white,
                      size: 20,
                    ),
                  ),
                ),
                
                const SizedBox(height: 30),
                
                // Items dinámicos del sidebar
                ...widget.sidebarItems.asMap().entries.map((entry) {
                  int index = entry.key;
                  SidebarItem item = entry.value;
                  
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 16),
                    child: _buildSidebarIcon(
                      icon: item.icon,
                      tooltip: item.name,
                      isSelected: _selectedIndex == index,
                      onTap: () {
                        setState(() => _selectedIndex = index);
                        item.onPress();
                      },
                    ),
                  );
                }).toList(),
                
                const Spacer(),
                
                // Icono Logout (condicional)
                if (widget.showLogout)
                  _buildSidebarIcon(
                    icon: Icons.logout,
                    tooltip: 'Cerrar Sesión',
                    isSelected: false,
                    onTap: _logout,
                    isLogout: true,
                  ),
                
                const SizedBox(height: 20),
              ],
            ),
          ),
          
          // Contenido principal
          Expanded(
            child: widget.body,
          ),
        ],
      ),
    );
  }

  Widget _buildSidebarIcon({
    required IconData icon,
    required String tooltip,
    required bool isSelected,
    required VoidCallback onTap,
    bool isLogout = false,
  }) {
    return Tooltip(
      message: tooltip,
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(8),
          child: Container(
            width: 44,
            height: 44,
            decoration: BoxDecoration(
              color: isSelected 
                  ? AppColors.primaryColor 
                  : Colors.transparent,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Icon(
              icon,
              color: isLogout 
                  ? Colors.red.shade700
                  : isSelected 
                      ? Colors.white
                      : Colors.black87,
              size: 20,
            ),
          ),
        ),
      ),
    );
  }
}

class SidebarItem {
  final String name;
  final IconData icon;
  final VoidCallback onPress;

  SidebarItem({
    required this.name,
    required this.icon,
    required this.onPress,
  });
}