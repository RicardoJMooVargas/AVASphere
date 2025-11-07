import 'package:flutter/material.dart';
import 'package:get/get.dart';

class NotFoundScreen extends StatelessWidget {
  const NotFoundScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline, size: 64, color: Colors.red),
            const SizedBox(height: 16),
            const Text('Página no encontrada'),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: () => Get.offAllNamed('/login'),
              child: const Text('Ir a Login'),
            ),
          ],
        ),
      ),
    );
  }
}
