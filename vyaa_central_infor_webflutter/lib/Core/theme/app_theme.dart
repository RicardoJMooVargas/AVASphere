import 'package:flutter/material.dart';
import 'app_colors.dart';
class AppTheme {
  // Tema único consistente
  static ThemeData get theme => ThemeData(
    useMaterial3: true,
    
    // Colores principales
    colorScheme: ColorScheme.light(
      primary: AppColors.primaryColor,
      secondary: AppColors.tertiaryColor,
      surface: Colors.white,
    ),

    // ExpansionTile con primaryColor - CONFIGURACIÓN PRINCIPAL
    expansionTileTheme: ExpansionTileThemeData(
      backgroundColor: Colors.white, // Fondo azul cuando expandido
      collapsedBackgroundColor: Colors.white, // Fondo azul cuando colapsado
      textColor: Colors.white, // Texto blanco cuando expandido
      collapsedTextColor: Colors.white, // Texto blanco cuando colapsado
      iconColor: Colors.black, // Icono negro cuando expandido
      collapsedIconColor: Colors.black, // Icono negro cuando colapsado
      tilePadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      childrenPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.all(Radius.circular(8)),
      ),
      collapsedShape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.all(Radius.circular(8)),
      ),
    ),

    // Cards
    cardTheme: CardThemeData(
      color: Colors.white,
      elevation: 2,
      margin: const EdgeInsets.all(8),
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.all(Radius.circular(12)),
      ),
    ),

    // Inputs
    inputDecorationTheme: const InputDecorationTheme(
      filled: true,
      fillColor: Colors.white,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.all(Radius.circular(8)),
        borderSide: BorderSide(color: AppColors.primaryColorGray),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.all(Radius.circular(8)),
        borderSide: BorderSide(color: AppColors.primaryColorGray),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.all(Radius.circular(8)),
        borderSide: BorderSide(color: AppColors.primaryColor, width: 2),
      ),
      labelStyle: TextStyle(color: AppColors.primaryColorGray),
      floatingLabelStyle: TextStyle(color: AppColors.primaryColor),
      hintStyle: TextStyle(color: Colors.grey),
      contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 12),
    ),

    // Botones
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        backgroundColor: AppColors.primaryColor,
        foregroundColor: Colors.white,
        shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.all(Radius.circular(8)),
        ),
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      ),
    ),

    // Texto general
    textTheme: const TextTheme(
      headlineLarge: TextStyle(color: AppColors.primaryColorGray, fontWeight: FontWeight.bold),
      headlineMedium: TextStyle(color: AppColors.primaryColorGray, fontWeight: FontWeight.bold),
      headlineSmall: TextStyle(color: AppColors.primaryColorGray, fontWeight: FontWeight.w600),
      titleLarge: TextStyle(color: AppColors.primaryColorGray, fontWeight: FontWeight.bold),
      titleMedium: TextStyle(color: AppColors.primaryColorGray, fontWeight: FontWeight.w600),
      titleSmall: TextStyle(color: AppColors.primaryColorGray, fontWeight: FontWeight.w500),
      bodyLarge: TextStyle(color: AppColors.primaryColorGray),
      bodyMedium: TextStyle(color: AppColors.primaryColorGray),
      bodySmall: TextStyle(color: AppColors.primaryColorGray),
      labelLarge: TextStyle(color: AppColors.primaryColorGray, fontWeight: FontWeight.w500),
      labelMedium: TextStyle(color: AppColors.primaryColorGray),
      labelSmall: TextStyle(color: AppColors.primaryColorGray),
    ),
  );

  // Método light que devuelve el tema único
  static ThemeData light() => theme;

  // Método dark que devuelve el mismo tema (sin modo oscuro por ahora)
  static ThemeData dark() => theme;
}