import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../services/data/hive.service.dart';

class NotFoundScreen extends StatefulWidget {
  const NotFoundScreen({super.key});

  @override
  State<NotFoundScreen> createState() => _NotFoundScreenState();
}

class _NotFoundScreenState extends State<NotFoundScreen> {
  bool _isChecking = true;
  String _redirectRoute = '/login';

  @override
  void initState() {
    super.initState();
    _determineRedirectRoute();
  }

  /// Determina a dónde redirigir según el estado de la sesión
  Future<void> _determineRedirectRoute() async {
    try {
      // Verificar si hay sesión válida
      final hasValidSession = await HiveService.hasValidSession();
      
      if (mounted) {
        setState(() {
          _redirectRoute = hasValidSession ? '/app/home' : '/login';
          _isChecking = false;
        });
      }
    } catch (e) {
      debugPrint('❌ Error determinando redirección: $e');
      if (mounted) {
        setState(() {
          _redirectRoute = '/login';
          _isChecking = false;
        });
      }
    }
  }

  void _navigateToDestination() {
    if (mounted) {
      context.go(_redirectRoute);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(
              Icons.error_outline,
              size: 64,
              color: Colors.red,
            ),
            const SizedBox(height: 24),
            const Text(
              '404',
              style: TextStyle(
                fontSize: 48,
                fontWeight: FontWeight.bold,
                color: Colors.red,
              ),
            ),
            const SizedBox(height: 8),
            const Text(
              'Página no encontrada',
              style: TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.w500,
              ),
            ),
            const SizedBox(height: 8),
            const Padding(
              padding: EdgeInsets.symmetric(horizontal: 32),
              child: Text(
                'La página que buscas no existe o ha sido movida',
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: Colors.grey,
                ),
              ),
            ),
            const SizedBox(height: 32),
            if (_isChecking)
              const CircularProgressIndicator()
            else
              ElevatedButton.icon(
                onPressed: _navigateToDestination,
                icon: Icon(
                  _redirectRoute == '/login' ? Icons.login : Icons.home,
                ),
                label: Text(
                  _redirectRoute == '/login' ? 'Ir a Login' : 'Ir al Inicio',
                ),
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 32,
                    vertical: 16,
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }
}