import 'package:flutter/material.dart';
import '../../../Core/theme/app_colors.dart';
import '../../../Core/Widgets/system/app_navbar.widget.dart';

class SupplyPage extends StatelessWidget {
  const SupplyPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey.shade50,
      appBar: AppNavbarWidget(
        title: 'Módulo de Suministros',
        backgroundColor: AppColors.tertiaryColor,
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                color: AppColors.tertiaryColor.withOpacity(0.1),
                borderRadius: BorderRadius.circular(20),
              ),
              child: Icon(
                Icons.local_shipping,
                size: 80,
                color: AppColors.tertiaryColor,
              ),
            ),
            
            const SizedBox(height: 32),
            
            Text(
              'Módulo de Suministros',
              style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                color: AppColors.tertiaryColor,
                fontWeight: FontWeight.bold,
              ),
            ),
            
            const SizedBox(height: 16),
            
            Text(
              'Control de proveedores y suministros',
              style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                color: Colors.grey.shade600,
              ),
            ),
            
            const SizedBox(height: 8),
            
            Text(
              'En desarrollo',
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                color: Colors.grey.shade500,
              ),
            ),
          ],
        ),
      ),
    );
  }
}