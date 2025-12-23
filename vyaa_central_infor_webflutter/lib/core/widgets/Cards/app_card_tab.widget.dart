import 'package:flutter/material.dart';
import '../../theme/app_colors.dart';

/// Widget de card con pestañas (tabs) que permite navegación reactiva entre secciones
///
/// Este widget proporciona una interfaz de pestañas dentro de un card,
/// permitiendo cambiar entre diferentes secciones de contenido de manera reactiva.
///
/// Ejemplo de uso:
/// ```dart
/// AppCardTab(
///   tabs: [
///     TabData(
///       label: 'Información',
///       icon: Icons.info,
///       content: InfoWidget(),
///     ),
///     TabData(
///       label: 'Configuración',
///       icon: Icons.settings,
///       content: SettingsWidget(),
///     ),
///   ],
///   onTabChanged: (index) {
///     print('Tab cambiada a: $index');
///   },
/// )
/// ```
class AppCardTab extends StatefulWidget {
  /// Lista de datos de las pestañas
  final List<TabData> tabs;

  /// Índice inicial de la pestaña seleccionada
  final int initialIndex;

  /// Callback que se ejecuta cuando cambia la pestaña
  final ValueChanged<int>? onTabChanged;

  /// Altura del card
  final double? height;

  /// Radio de borde del card
  final double borderRadius;

  /// Color de fondo del card
  final Color? backgroundColor;

  /// Color del indicador de la pestaña seleccionada
  final Color? indicatorColor;

  /// Color del texto de las pestañas
  final Color? tabTextColor;

  /// Color del texto de la pestaña seleccionada
  final Color? selectedTabTextColor;

  /// Estilo del texto de las pestañas
  final TextStyle? tabTextStyle;

  /// Padding interno del card
  final EdgeInsetsGeometry? padding;

  /// Padding del contenido de las pestañas
  final EdgeInsetsGeometry? contentPadding;

  /// Padding del footer de las pestañas
  final EdgeInsetsGeometry? footerPadding;

  /// Color de fondo del footer
  final Color? footerBackgroundColor;

  /// Si se debe mostrar una línea divisoria entre el contenido y el footer
  final bool showFooterDivider;

  const AppCardTab({
    Key? key,
    required this.tabs,
    this.initialIndex = 0,
    this.onTabChanged,
    this.height,
    this.borderRadius = 12.0,
    this.backgroundColor,
    this.indicatorColor,
    this.tabTextColor,
    this.selectedTabTextColor,
    this.tabTextStyle,
    this.padding,
    this.contentPadding,
    this.footerPadding,
    this.footerBackgroundColor,
    this.showFooterDivider = true,
  }) : super(key: key);

  @override
  State<AppCardTab> createState() => _AppCardTabState();
}

class _AppCardTabState extends State<AppCardTab> with TickerProviderStateMixin {
  late TabController _tabController;
  late int _currentIndex;

  @override
  void initState() {
    super.initState();
    _currentIndex = widget.initialIndex;
    _tabController = TabController(
      length: widget.tabs.length,
      vsync: this,
      initialIndex: widget.initialIndex,
    );

    _tabController.addListener(_handleTabChange);
  }

  @override
  void didUpdateWidget(AppCardTab oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.tabs.length != widget.tabs.length) {
      _tabController.dispose();
      _tabController = TabController(
        length: widget.tabs.length,
        vsync: this,
        initialIndex: widget.initialIndex.clamp(0, widget.tabs.length - 1),
      );
      _tabController.addListener(_handleTabChange);
    }
  }

  void _handleTabChange() {
    if (_currentIndex != _tabController.index) {
      setState(() {
        _currentIndex = _tabController.index;
      });
      widget.onTabChanged?.call(_currentIndex);
    }
  }

  /// Construye el contenido de una pestaña incluyendo el footer opcional
  Widget _buildTabContent(BuildContext context, TabData tab) {
    final theme = Theme.of(context);

    if (tab.footer == null) {
      // Si no hay footer, devolver solo el contenido
      return Padding(
        padding: const EdgeInsets.all(8.0),
        child: tab.content,
      );
    }

    // Si hay footer, construir layout con contenido y footer
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
      children: [
        // Contenido principal de la tab
        Expanded(
          child: tab.content,
        ),

        // Divisor opcional entre contenido y footer
        if (widget.showFooterDivider)
          Container(
            height: 1,
            margin: const EdgeInsets.symmetric(vertical: 8),
            color: theme.dividerColor.withValues(alpha: 0.3),
          ),
        // Footer de la tab
        Container(
          width: double.infinity,
          padding: widget.footerPadding ?? const EdgeInsets.all(8),
          decoration: widget.footerBackgroundColor != null
              ? BoxDecoration(
                  color: widget.footerBackgroundColor,
                  borderRadius: BorderRadius.only(
                    bottomLeft: Radius.circular(widget.borderRadius),
                    bottomRight: Radius.circular(widget.borderRadius),
                  ),
                )
              : null,
          child: tab.footer!,
        ),
      ],
      ),
    );
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(widget.borderRadius),
      ),
      color: widget.backgroundColor ?? theme.cardColor,
      child: Container(
        height: widget.height,
        padding: widget.padding ?? EdgeInsets.zero,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Tabs
            Container(
              decoration: BoxDecoration(
                border: Border(
                  bottom: BorderSide(
                    color: theme.dividerColor.withValues(alpha: 0.3),
                    width: 1,
                  ),
                ),
              ),
              child: TabBar(
                controller: _tabController,
                isScrollable: widget.tabs.length > 3, // Scroll si hay más de 3 tabs
                indicatorColor: widget.indicatorColor ?? AppColors.primaryColor,
                indicatorWeight: 3,
                labelColor: widget.selectedTabTextColor ?? AppColors.primaryColor,
                unselectedLabelColor: widget.tabTextColor ?? theme.textTheme.bodyMedium?.color?.withValues(alpha: 0.7),
                labelStyle: widget.tabTextStyle ?? theme.textTheme.bodyMedium?.copyWith(
                  fontWeight: FontWeight.w600,
                ),
                unselectedLabelStyle: widget.tabTextStyle ?? theme.textTheme.bodyMedium,
                tabs: widget.tabs.map((tab) => Tab(
                  text: tab.label,
                  icon: tab.icon != null ? Icon(tab.icon, size: 20) : null,
                  iconMargin: tab.icon != null ? const EdgeInsets.only(bottom: 4) : EdgeInsets.zero,
                )).toList(),
              ),
            ),

            // Contenido de las tabs
            Expanded(
              child: Container(
                padding: widget.contentPadding ?? const EdgeInsets.only(top: 16),
                child: TabBarView(
                  controller: _tabController,
                  children: widget.tabs.map((tab) => _buildTabContent(context, tab)).toList(),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Datos de una pestaña individual
///
/// Contiene la información necesaria para renderizar una pestaña
/// incluyendo etiqueta, ícono opcional, contenido y footer opcional.
class TabData {
  /// Etiqueta de texto de la pestaña
  final String label;

  /// Ícono opcional para la pestaña
  final IconData? icon;

  /// Widget de contenido que se muestra cuando la pestaña está seleccionada
  final Widget content;

  /// Widget opcional que se muestra en la parte inferior de la pestaña
  /// Útil para botones de acción, información adicional, etc.
  final Widget? footer;

  /// Tooltip opcional para la pestaña
  final String? tooltip;

  const TabData({
    required this.label,
    this.icon,
    required this.content,
    this.footer,
    this.tooltip,
  });
}

/// Widget auxiliar para crear contenido de pestañas con scroll automático
///
/// Útil cuando el contenido de una pestaña puede ser más alto que el espacio disponible.
class TabContent extends StatelessWidget {
  /// Contenido de la pestaña
  final Widget child;

  /// Padding del contenido
  final EdgeInsetsGeometry? padding;

  /// Si debe permitir scroll cuando el contenido es muy alto
  final bool scrollable;

  const TabContent({
    Key? key,
    required this.child,
    this.padding,
    this.scrollable = true,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final content = Padding(
      padding: padding ?? EdgeInsets.zero,
      child: child,
    );

    if (scrollable) {
      return SingleChildScrollView(
        padding: EdgeInsets.zero,
        child: content,
      );
    }

    return content;
  }
}

/// Utilidades para crear AppCardTab con datos dinámicos
class AppCardTabUtils {
  /// Crea un AppCardTab desde una lista de mapas con datos dinámicos
  ///
  /// Útil cuando los datos de las pestañas vienen de una API o base de datos.
  ///
  /// Ejemplo:
  /// ```dart
  /// final tabData = [
  ///   {
  ///     'label': 'Productos',
  ///     'icon': Icons.inventory,
  ///     'content': ProductsList(),
  ///     'footer': ActionButtons(),
  ///   },
  ///   {
  ///     'label': 'Clientes',
  ///     'icon': Icons.people,
  ///     'content': ClientsList(),
  ///   },
  /// ];
  ///
  /// AppCardTabUtils.fromDynamicData(tabData);
  /// ```
  static AppCardTab fromDynamicData(
    List<Map<String, dynamic>> data, {
    int initialIndex = 0,
    ValueChanged<int>? onTabChanged,
    double? height,
    double borderRadius = 12.0,
    Color? backgroundColor,
    Color? indicatorColor,
    Color? tabTextColor,
    Color? selectedTabTextColor,
    TextStyle? tabTextStyle,
    EdgeInsetsGeometry? padding,
    EdgeInsetsGeometry? contentPadding,
    EdgeInsetsGeometry? footerPadding,
    Color? footerBackgroundColor,
    bool showFooterDivider = true,
  }) {
    final tabs = data.map((item) => TabData(
      label: item['label'] as String,
      icon: item['icon'] as IconData?,
      content: item['content'] as Widget,
      footer: item['footer'] as Widget?,
      tooltip: item['tooltip'] as String?,
    )).toList();

    return AppCardTab(
      tabs: tabs,
      initialIndex: initialIndex,
      onTabChanged: onTabChanged,
      height: height,
      borderRadius: borderRadius,
      backgroundColor: backgroundColor,
      indicatorColor: indicatorColor,
      tabTextColor: tabTextColor,
      selectedTabTextColor: selectedTabTextColor,
      tabTextStyle: tabTextStyle,
      padding: padding,
      contentPadding: contentPadding,
      footerPadding: footerPadding,
      footerBackgroundColor: footerBackgroundColor,
      showFooterDivider: showFooterDivider,
    );
  }
}

/// Versión avanzada del AppCardTab que permite control externo reactivo
///
/// Esta versión permite cambiar la pestaña seleccionada desde fuera del widget
/// usando un ValueNotifier, lo que facilita la integración con controladores GetX
/// o cualquier otro sistema de estado reactivo.
///
/// Ejemplo de uso con GetX:
/// ```dart
/// class MyController extends GetxController {
///   final tabIndex = 0.obs;
///
///   void changeTab(int index) {
///     tabIndex.value = index;
///   }
/// }
///
/// class MyWidget extends StatelessWidget {
///   final controller = Get.put(MyController());
///
///   @override
///   Widget build(BuildContext context) {
///     return Obx(() => AppCardTabReactive(
///       tabs: [...],
///       currentIndex: controller.tabIndex.value,
///       onTabChanged: (index) {
///         controller.changeTab(index);
///       },
///     ));
///   }
/// }
/// ```
class AppCardTabReactive extends StatefulWidget {
  /// Lista de datos de las pestañas
  final List<TabData> tabs;

  /// Índice actual de la pestaña seleccionada (controlado externamente)
  final int currentIndex;

  /// Callback que se ejecuta cuando cambia la pestaña
  final ValueChanged<int>? onTabChanged;

  /// Altura del card
  final double? height;

  /// Radio de borde del card
  final double borderRadius;

  /// Color de fondo del card
  final Color? backgroundColor;

  /// Color del indicador de la pestaña seleccionada
  final Color? indicatorColor;

  /// Color del texto de las pestañas
  final Color? tabTextColor;

  /// Color del texto de la pestaña seleccionada
  final Color? selectedTabTextColor;

  /// Estilo del texto de las pestañas
  final TextStyle? tabTextStyle;

  /// Padding interno del card
  final EdgeInsetsGeometry? padding;

  /// Padding del contenido de las pestañas
  final EdgeInsetsGeometry? contentPadding;

  /// Duración de la animación de cambio de pestaña
  final Duration animationDuration;

  const AppCardTabReactive({
    Key? key,
    required this.tabs,
    required this.currentIndex,
    this.onTabChanged,
    this.height,
    this.borderRadius = 12.0,
    this.backgroundColor,
    this.indicatorColor,
    this.tabTextColor,
    this.selectedTabTextColor,
    this.tabTextStyle,
    this.padding,
    this.contentPadding,
    this.animationDuration = const Duration(milliseconds: 300),
  }) : super(key: key);

  @override
  State<AppCardTabReactive> createState() => _AppCardTabReactiveState();
}

class _AppCardTabReactiveState extends State<AppCardTabReactive> with TickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(
      length: widget.tabs.length,
      vsync: this,
      initialIndex: widget.currentIndex.clamp(0, widget.tabs.length - 1),
    );

    _tabController.addListener(_handleTabChange);
  }

  @override
  void didUpdateWidget(AppCardTabReactive oldWidget) {
    super.didUpdateWidget(oldWidget);

    // Actualizar el número de tabs si cambió
    if (oldWidget.tabs.length != widget.tabs.length) {
      _tabController.dispose();
      _tabController = TabController(
        length: widget.tabs.length,
        vsync: this,
        initialIndex: widget.currentIndex.clamp(0, widget.tabs.length - 1),
      );
      _tabController.addListener(_handleTabChange);
    }

    // Cambiar la tab si el índice cambió externamente
    if (oldWidget.currentIndex != widget.currentIndex &&
        widget.currentIndex != _tabController.index) {
      _tabController.animateTo(
        widget.currentIndex.clamp(0, widget.tabs.length - 1),
        duration: widget.animationDuration,
      );
    }
  }

  void _handleTabChange() {
    // Solo notificar si el cambio no vino del widget padre
    if (_tabController.index != widget.currentIndex) {
      widget.onTabChanged?.call(_tabController.index);
    }
  }

  /// Construye el contenido de una pestaña incluyendo el footer opcional
  Widget _buildTabContent(BuildContext context, TabData tab) {
    final theme = Theme.of(context);

    if (tab.footer == null) {
      // Si no hay footer, devolver solo el contenido
      return Padding(
        padding: const EdgeInsets.all(8.0),
        child: tab.content,
      );
    }

    // Si hay footer, construir layout con contenido y footer
    return Padding(
      padding: const EdgeInsets.all(8.0),
      child: Column(
      children: [
        // Contenido principal de la tab
        Expanded(
          child: tab.content,
        ),

        // Divisor entre contenido y footer
        Container(
          height: 1,
          margin: const EdgeInsets.symmetric(vertical: 8),
          color: theme.dividerColor.withValues(alpha: 0.3),
        ),

        // Footer de la tab
        Container(
          width: double.infinity,
          padding: const EdgeInsets.all(8),
          child: tab.footer!,
        ),
      ],
      ),
    );
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(widget.borderRadius),
      ),
      color: widget.backgroundColor ?? theme.cardColor,
      child: Container(
        height: widget.height,
        padding: widget.padding ?? EdgeInsets.zero,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Tabs
            Container(
              decoration: BoxDecoration(
                border: Border(
                  bottom: BorderSide(
                    color: theme.dividerColor.withValues(alpha: 0.3),
                    width: 1,
                  ),
                ),
              ),
              child: TabBar(
                controller: _tabController,
                isScrollable: widget.tabs.length > 3,
                indicatorColor: widget.indicatorColor ?? AppColors.primaryColor,
                indicatorWeight: 3,
                labelColor: widget.selectedTabTextColor ?? AppColors.primaryColor,
                unselectedLabelColor: widget.tabTextColor ?? theme.textTheme.bodyMedium?.color?.withValues(alpha: 0.7),
                labelStyle: widget.tabTextStyle ?? theme.textTheme.bodyMedium?.copyWith(
                  fontWeight: FontWeight.w600,
                ),
                unselectedLabelStyle: widget.tabTextStyle ?? theme.textTheme.bodyMedium,
                tabs: widget.tabs.map((tab) => Tab(
                  text: tab.label,
                  icon: tab.icon != null ? Icon(tab.icon, size: 20) : null,
                  iconMargin: tab.icon != null ? const EdgeInsets.only(bottom: 4) : EdgeInsets.zero,
                )).toList(),
              ),
            ),

            // Contenido de las tabs
            Expanded(
              child: Container(
                padding: widget.contentPadding ?? const EdgeInsets.only(top: 16),
                child: TabBarView(
                  controller: _tabController,
                  children: widget.tabs.map((tab) => _buildTabContent(context, tab)).toList(),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Controlador para gestionar el estado de las pestañas de manera reactiva
///
/// Facilita la integración con sistemas de estado como GetX, Provider, etc.
/// Permite cambiar pestañas desde cualquier parte de la aplicación.
///
/// Ejemplo de uso con GetX:
/// ```dart
/// class TabController extends GetxController {
///   final tabController = AppTabController();
///
///   void changeToProductsTab() {
///     tabController.changeTab(0);
///   }
///
///   void changeToClientsTab() {
///     tabController.changeTab(1);
///   }
/// }
/// ```
class AppTabController {
  final ValueNotifier<int> _currentTab = ValueNotifier<int>(0);

  /// Stream del índice actual de la pestaña
  ValueNotifier<int> get currentTab => _currentTab;

  /// Obtiene el índice actual de la pestaña
  int get currentIndex => _currentTab.value;

  /// Cambia a una pestaña específica
  void changeTab(int index) {
    if (index != _currentTab.value) {
      _currentTab.value = index;
    }
  }

  /// Cambia a la siguiente pestaña
  void nextTab(int maxTabs) {
    final nextIndex = (_currentTab.value + 1) % maxTabs;
    changeTab(nextIndex);
  }

  /// Cambia a la pestaña anterior
  void previousTab(int maxTabs) {
    final prevIndex = _currentTab.value - 1;
    changeTab(prevIndex < 0 ? maxTabs - 1 : prevIndex);
  }

  /// Resetea a la primera pestaña
  void reset() {
    changeTab(0);
  }

  /// Libera los recursos
  void dispose() {
    _currentTab.dispose();
  }
}
