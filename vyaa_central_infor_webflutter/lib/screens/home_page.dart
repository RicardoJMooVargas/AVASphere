import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../services/local/cache_service.dart';

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  Future<void> _logout(BuildContext context) async {
    await CacheService.removeToken();
    if (context.mounted) {
      GoRouter.of(context).go('/login');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Home')),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Text('Welcome! You are logged in.'),
            const SizedBox(height: 20),
            ElevatedButton(
              onPressed: () => _logout(context),
              child: const Text('Logout'),
            ),
          ],
        ),
      ),
    );
  }
}
