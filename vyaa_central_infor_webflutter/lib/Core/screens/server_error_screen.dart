import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../services/system_init.service.dart';
import '../controllers/server_status.controller.dart';
import '../middlewares/global_init.middleware.dart';

class ServerErrorScreen extends StatefulWidget {
  const ServerErrorScreen({super.key});

  @override
  State<ServerErrorScreen> createState() => _ServerErrorScreenState();
}

class _ServerErrorScreenState extends State<ServerErrorScreen> {
  bool _isRetrying = false;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [
              Colors.red.shade400,
              Colors.red.shade600,
            ],
          ),
        ),
        child: SafeArea(
          child: Padding(
            padding: const EdgeInsets.all(24.0),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                // Icono de error
                Container(
                  padding: const EdgeInsets.all(20),
                  decoration: BoxDecoration(
                    color: Colors.white.withOpacity(0.2),
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(
                    Icons.cloud_off,
                    size: 80,
                    color: Colors.white,
                  ),
                ),
                
                const SizedBox(height: 32),
                
                // Título
                const Text(
                  'Servidor No Disponible',
                  style: TextStyle(
                    fontSize: 28,
                    fontWeight: FontWeight.bold,
                    color: Colors.white,
                  ),
                  textAlign: TextAlign.center,
                ),
                
                const SizedBox(height: 16),
                
                // Descripción
                const Text(
                  'No se pudo establecer conexión con el servidor. Por favor, verifica que:',
                  style: TextStyle(
                    fontSize: 16,
                    color: Colors.white,
                  ),
                  textAlign: TextAlign.center,
                ),
                
                const SizedBox(height: 24),
                
                // Lista de verificación
                Container(
                  padding: const EdgeInsets.all(20),
                  decoration: BoxDecoration(
                    color: Colors.white.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      _ChecklistItem(text: 'El servidor esté ejecutándose'),
                      _ChecklistItem(text: 'La URL de conexión sea correcta'),
                      _ChecklistItem(text: 'No haya problemas de red'),
                      _ChecklistItem(text: 'El firewall no esté bloqueando la conexión'),
                    ],
                  ),
                ),
                
                const SizedBox(height: 32),
                
                // Botones de acción
                Column(
                  children: [
                    // Botón principal - Reintentar
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton.icon(
                        onPressed: _isRetrying ? null : _retryConnection,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Colors.white,
                          foregroundColor: Colors.red.shade600,
                          padding: const EdgeInsets.symmetric(vertical: 16),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(8),
                          ),
                        ),
                        icon: _isRetrying 
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                valueColor: AlwaysStoppedAnimation<Color>(Colors.red),
                              ),
                            )
                          : const Icon(Icons.refresh),
                        label: Text(_isRetrying ? 'Reintentando...' : 'Reintentar Conexión'),
                      ),
                    ),
                    
                    const SizedBox(height: 12),
                    
                    // Botón secundario - Ver configuración
                    SizedBox(
                      width: double.infinity,
                      child: OutlinedButton.icon(
                        onPressed: _showConfiguration,
                        style: OutlinedButton.styleFrom(
                          foregroundColor: Colors.white,
                          side: const BorderSide(color: Colors.white),
                          padding: const EdgeInsets.symmetric(vertical: 16),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(8),
                          ),
                        ),
                        icon: const Icon(Icons.settings),
                        label: const Text('Ver Configuración'),
                      ),
                    ),
                    
                    const SizedBox(height: 12),
                    
                    // Botón terciario - Solo para desarrollo
                    if (kDebugMode)
                      TextButton.icon(
                        onPressed: _continueWithoutVerification,
                        style: TextButton.styleFrom(
                          foregroundColor: Colors.white.withOpacity(0.8),
                        ),
                        icon: const Icon(Icons.arrow_forward),
                        label: const Text('Continuar sin verificar (Debug)'),
                      ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _retryConnection() async {
    setState(() {
      _isRetrying = true;
    });

    try {
      debugPrint('🔄 Reintentando conexión con el servidor...');
      
      // Resetear el estado de inicialización para forzar nueva verificación
      GlobalInitMiddleware.reset();
      
      // Volver a la pantalla de inicialización para verificar el estado
      Get.offAllNamed('/');
      
    } catch (e) {
      debugPrint('❌ Error al reintentar: $e');
      // Si hay error, mostrar mensaje (opcional)
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Error al reintentar: ${e.toString()}'),
            backgroundColor: Colors.red.shade700,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isRetrying = false;
        });
      }
    }
  }

  void _showConfiguration() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Configuración del Servidor'),
        content: const Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('URL del Servidor:'),
            SizedBox(height: 8),
            SelectableText(
              'https://localhost:7100',
              style: TextStyle(
                fontFamily: 'Courier',
                backgroundColor: Color(0xFFF5F5F5),
              ),
            ),
            SizedBox(height: 16),
            Text('Endpoint de verificación:'),
            SizedBox(height: 8),
            SelectableText(
              '/api/system/Config/check-initial-config',
              style: TextStyle(
                fontFamily: 'Courier',
                backgroundColor: Color(0xFFF5F5F5),
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cerrar'),
          ),
        ],
      ),
    );
  }

  void _continueWithoutVerification() {
    debugPrint('🚧 Continuando sin verificación de servidor (modo debug)');
    
    // Marcar servidor como disponible y continuar al login
    final serverStatus = Get.find<ServerStatusController>();
    serverStatus.markServerAvailable();
    
    Get.offAllNamed('/login');
  }
}

class _ChecklistItem extends StatelessWidget {
  final String text;

  const _ChecklistItem({required this.text});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          const Icon(
            Icons.check_circle_outline,
            color: Colors.white,
            size: 20,
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              text,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 14,
              ),
            ),
          ),
        ],
      ),
    );
  }
}