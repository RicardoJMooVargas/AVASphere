import 'package:flutter/material.dart';
import 'package:vyaa_central_infor_webflutter/core/theme/app_colors.dart';

class LoginHeader extends StatelessWidget {
  final String title;
  final String subtitle;
  final IconData icon;

  const LoginHeader({
    Key? key,
    required this.title,
    required this.subtitle,
    this.icon = Icons.build,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        // Icono
        Container(
          width: 120,
          height: 120,
          decoration: BoxDecoration(
            color: AppColors.primaryColorWithOpacity,
            shape: BoxShape.circle,
          ),
          child: Icon(
            icon,
            size: 60,
            color: AppColors.primaryColor,
          ),
        ),
        const SizedBox(height: 24),
        
        // Título
        Text(
          title,
          style: const TextStyle(
            fontSize: 32,
            color: AppColors.tertiaryColor,
            fontWeight: FontWeight.bold,
            letterSpacing: 1.5,
          ),
        ),
        
        // Subtítulo
        Text(
          subtitle,
          style: const TextStyle(
            fontSize: 16,
            color: Colors.white,
            fontWeight: FontWeight.w400,
          ),
        ),
      ],
    );
  }
}