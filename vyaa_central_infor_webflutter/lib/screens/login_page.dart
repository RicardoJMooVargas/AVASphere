import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../services/local/cache_service.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final TextEditingController _usernameController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  bool _loading = false;

  Future<void> _fakeLogin() async {
    setState(() => _loading = true);
    // Simulate an API call and return a fake token
    await Future.delayed(const Duration(seconds: 1));
    const fakeToken = 'fake_token_123';
    await CacheService.saveToken(fakeToken);
    setState(() => _loading = false);
    // Navigate to home
    if (mounted) {
      GoRouter.of(context).go('/home');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Login')),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            TextField(
              controller: _usernameController,
              decoration: const InputDecoration(labelText: 'Username'),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _passwordController,
              decoration: const InputDecoration(labelText: 'Password'),
              obscureText: true,
            ),
            const SizedBox(height: 20),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: _loading ? null : _fakeLogin,
                child: _loading ? const CircularProgressIndicator() : const Text('Sign in'),
              ),
            )
          ],
        ),
      ),
    );
  }
}
