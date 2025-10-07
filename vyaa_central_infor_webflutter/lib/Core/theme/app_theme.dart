import 'package:flutter/material.dart';
import 'app_colors.dart';

class AppTheme {
  static ThemeData light() {
    return ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.light(
        primary: AppColors.primaryColor,
        secondary: AppColors.secondary,
        tertiary: AppColors.tertiaryColor,
        surface: Colors.white,
        background: const Color(0xFFF5F5F5),
      ),
      
      // Input decoration theme
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
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.all(Radius.circular(8)),
          borderSide: BorderSide(color: Colors.red, width: 1),
        ),
        focusedErrorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.all(Radius.circular(8)),
          borderSide: BorderSide(color: Colors.red, width: 2),
        ),
        labelStyle: TextStyle(color: AppColors.primaryColorGray),
        floatingLabelStyle: TextStyle(color: AppColors.primaryColor),
        hintStyle: TextStyle(color: AppColors.primaryColorGray),
        contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      ),

      // Card theme
      cardTheme: CardThemeData(
        color: Colors.white,
        elevation: 2,
        margin: const EdgeInsets.all(8),
        shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.all(Radius.circular(12)),
        ),
      ),

      // Expansion tile theme
      expansionTileTheme: const ExpansionTileThemeData(
        backgroundColor: Colors.white,
        collapsedBackgroundColor: Colors.white,
        textColor: AppColors.primaryColor,
        collapsedTextColor: AppColors.primaryColorGray,
        iconColor: AppColors.primaryColor,
        collapsedIconColor: AppColors.primaryColorGray,
      ),

      // Date picker theme
      datePickerTheme: DatePickerThemeData(
        backgroundColor: Colors.white,
        headerBackgroundColor: AppColors.primaryColor,
        headerForegroundColor: Colors.white,
        dayForegroundColor: WidgetStateProperty.all(AppColors.primaryColorGray),
        todayForegroundColor: WidgetStateProperty.all(AppColors.primaryColor),
        dayBackgroundColor: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return AppColors.primaryColor;
          }
          return null;
        }),
      ),

      // Time picker theme
      timePickerTheme: const TimePickerThemeData(
        backgroundColor: Colors.white,
        hourMinuteTextColor: AppColors.primaryColor,
        dayPeriodTextColor: AppColors.primaryColor,
        dialHandColor: AppColors.primaryColor,
        dialBackgroundColor: AppColors.primaryColorGray,
      ),

      // Button themes
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

      // Icon theme
      iconTheme: const IconThemeData(
        color: AppColors.primaryColorGray,
      ),

      // Text theme
      textTheme: const TextTheme(
        headlineLarge: TextStyle(color: AppColors.primaryColorGray),
        headlineMedium: TextStyle(color: AppColors.primaryColorGray),
        headlineSmall: TextStyle(color: AppColors.primaryColorGray),
        titleLarge: TextStyle(color: AppColors.primaryColorGray, fontWeight: FontWeight.bold),
        titleMedium: TextStyle(color: AppColors.primaryColorGray, fontWeight: FontWeight.w600),
        titleSmall: TextStyle(color: AppColors.primaryColorGray),
        bodyLarge: TextStyle(color: AppColors.primaryColorGray),
        bodyMedium: TextStyle(color: AppColors.primaryColorGray),
        bodySmall: TextStyle(color: AppColors.primaryColorGray),
        labelLarge: TextStyle(color: AppColors.primaryColorGray),
        labelMedium: TextStyle(color: AppColors.primaryColorGray),
        labelSmall: TextStyle(color: AppColors.primaryColorGray),
      ),
    );
  }

  static ThemeData dark() {
    return ThemeData.dark();
  }
}
