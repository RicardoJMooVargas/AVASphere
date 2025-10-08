import 'package:flutter/material.dart';

/// Layout principal de la aplicación con diseño de 3 secciones.
/// 
/// Estructura:
/// - Columna izquierda: Sidebar/navegación
/// - Columna derecha superior: Header/información rápida
/// - Columna derecha inferior: Body principal para contenido/tablas
/// 
/// La proporción es aproximadamente:
/// - Columna izquierda: 30% del ancho
/// - Columna derecha: 70% del ancho
///   - Header derecho: 15% de la altura
///   - Body derecho: 85% de la altura
class HomeLayout extends StatelessWidget {
  /// Widget para la columna izquierda (sidebar).
  final Widget leftColumn;
  
  /// Widget para la sección superior derecha (header).
  final Widget rightHeader;
  
  /// Widget para la sección inferior derecha (body principal).
  final Widget rightBody;

  /// Color de fondo del layout.
  final Color? backgroundColor;

  /// Color de los bordes entre secciones.
  final Color borderColor;

  /// Ancho de los bordes entre secciones.
  final double borderWidth;

  /// Crea un [HomeLayout] con las tres secciones especificadas.
  /// 
  /// Ejemplo de uso:
  /// ```dart
  /// HomeLayout(
  ///   leftColumn: MySidebar(),
  ///   rightHeader: MyHeader(),
  ///   rightBody: MyDataTable(),
  /// )
  /// ```
  const HomeLayout({
    Key? key,
    required this.leftColumn,
    required this.rightHeader,
    required this.rightBody,
    this.backgroundColor,
    this.borderColor = Colors.transparent,
    this.borderWidth = 0.0,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      color: backgroundColor ?? Theme.of(context).scaffoldBackgroundColor,
      child: Row(
        children: [
          // === COLUMNA IZQUIERDA (30%) ===
          Expanded(
            flex: 3,
            child: Container(
              decoration: borderWidth > 0 ? BoxDecoration(
                border: Border(
                  right: BorderSide(
                    color: borderColor,
                    width: borderWidth,
                  ),
                ),
              ) : null,
              child: leftColumn,
            ),
          ),

          // === COLUMNA DERECHA (70%) ===
          Expanded(
            flex: 7,
            child: Column(
              children: [
                // --- HEADER DERECHO (10% altura) ---
                Expanded(
                  flex: 10,
                  child: Container(
                    width: double.infinity,
                    decoration: borderWidth > 0 ? BoxDecoration(
                      border: Border(
                        bottom: BorderSide(
                          color: borderColor,
                          width: borderWidth,
                        ),
                      ),
                    ) : null,
                    child: rightHeader,
                  ),
                ),

                // --- BODY DERECHO (90% altura) ---
                Expanded(
                  flex: 90,
                  child: Container(
                    width: double.infinity,
                    child: rightBody,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

/// Widget de placeholder para secciones en construcción.
/// 
/// Muestra un icono de construcción con texto informativo.
class UnderConstructionCard extends StatelessWidget {
  /// Título de la sección.
  final String title;
  
  /// Descripción opcional de la sección.
  final String? description;
  
  /// Color de fondo de la card.
  final Color? backgroundColor;
  
  /// Color del icono y texto.
  final Color? foregroundColor;

  /// Crea una card de "en construcción".
  const UnderConstructionCard({
    Key? key,
    required this.title,
    this.description,
    this.backgroundColor,
    this.foregroundColor,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final bgColor = backgroundColor ?? theme.cardColor;
    final fgColor = foregroundColor ?? theme.textTheme.bodyLarge?.color ?? Colors.black54;

    return Card(
      color: bgColor,
      elevation: 2,
      margin: const EdgeInsets.all(16),
      child: Container(
        width: double.infinity,
        padding: const EdgeInsets.all(18),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.construction,
              size: 64,
              color: fgColor.withOpacity(0.6),
            ),
            const SizedBox(height: 16),
            Text(
              title,
              style: theme.textTheme.headlineSmall?.copyWith(
                color: fgColor,
                fontWeight: FontWeight.bold,
              ),
              textAlign: TextAlign.center,
            ),
            if (description != null) ...[
              const SizedBox(height: 8),
              Text(
                description!,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: fgColor.withOpacity(0.8),
                ),
                textAlign: TextAlign.center,
              ),
            ],
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
              decoration: BoxDecoration(
                color: Colors.orange.withOpacity(0.1),
                borderRadius: BorderRadius.circular(16),
                border: Border.all(
                  color: Colors.orange.withOpacity(0.3),
                  width: 1,
                ),
              ),
              child: Text(
                'EN CONSTRUCCIÓN',
                style: theme.textTheme.labelSmall?.copyWith(
                  color: Colors.orange.shade700,
                  fontWeight: FontWeight.bold,
                  letterSpacing: 1.2,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Layout predefinido con cards de construcción para desarrollo rápido.
/// 
/// Útil para crear prototipos rápidos y mostrar el diseño general
/// mientras se desarrollan las secciones individuales.
class HomeLayoutDemo extends StatelessWidget {
  const HomeLayoutDemo({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: HomeLayout(
        leftColumn: const UnderConstructionCard(
          title: 'Navegación',
          description: 'Menú lateral y opciones de navegación',
        ),
        rightHeader: const UnderConstructionCard(
          title: 'Header',
          description: 'Información rápida, breadcrumbs, acciones',
        ),
        rightBody: const UnderConstructionCard(
          title: 'Contenido Principal',
          description: 'Aquí va la tabla de datos o contenido principal',
        ),
      ),
    );
  }
}
