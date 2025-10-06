import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:go_router/go_router.dart';
import '../services/api/auth.service.dart';
import '../models/requests/auth_request.model.dart';
import '../Core/theme/app_colors.dart';
import 'dart:ui';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  // Use model which owns controllers so we can reuse and dispose them cleanly
  final AuthRequest _authModel = AuthRequest();
  bool _loading = false;
  
  // Create a focus node for the password field
  final passwordFocusNode = FocusNode();
  
  // State to control password visibility
  bool _isPasswordVisible = false;
  
  // State to control button hover
  bool _isHovering = false;

  Future<void> _fakeLogin() async {
    setState(() => _loading = true);
    try {
      final service = AuthService();
  await service.login(_authModel);
      if (!mounted) return;
      GoRouter.of(context).go('/home');
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          _buildBackground(),
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  _buildLoginForm(context),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  // Background widget builder
  Widget _buildBackground() {
    return Positioned.fill(
      child: SvgPicture.asset(
        'assets/svg/background_login.svg',
        fit: BoxFit.cover,
      ),
    );
  }

  // Login form widget builder
  Widget _buildLoginForm(BuildContext context) {
    return SizedBox(
      width: 400,
      child: Card(
        elevation: 3,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(10),
        ),
        // Apply a white metallic transparent background to the card
        color: Colors.transparent,
        child: ClipRRect(
          borderRadius: BorderRadius.circular(10),
          child: BackdropFilter(
            filter: ImageFilter.blur(sigmaX: 5, sigmaY: 5),
            child: Container(
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(10),
                gradient: LinearGradient(
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                  colors: [
                    Colors.white.withOpacity(0.9),
                    Colors.white.withOpacity(0.6),
                  ],
                ),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.1),
                    spreadRadius: 1,
                    blurRadius: 3,
                    offset: const Offset(0, 2),
                  ),
                ],
                border: Border.all(
                  color: Colors.white.withOpacity(0.8),
                  width: 1,
                ),
              ),
              child: Padding(
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    // Logo
                    Image.asset('assets/icons/vyaa-solo-icon.png',
                        height: 150),
                    const SizedBox(height: 16),

                    // Title
                    const Text(
                      'VYAA Central Infor',
                      style: TextStyle(
                        fontSize: 24,
                        color: Color(0xFF2C3E50), // Dark blue-gray
                        fontWeight: FontWeight.bold,
                        letterSpacing: 1.2,
                      ),
                    ),
                    const SizedBox(height: 24),

                    // Username field
                    _buildUsernameField(),
                    const SizedBox(height: 16),

                    // Password field
                    _buildPasswordField(),
                    const SizedBox(height: 24),

                    // Login button
                    _buildLoginButton(),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  // Username field widget builder
  Widget _buildUsernameField() {
    return TextFormField(
      controller: _authModel.userNameController,
      decoration: const InputDecoration(
        labelText: 'Usuario',
        labelStyle: TextStyle(color: AppColors.primaryColorGray),
        enabledBorder: UnderlineInputBorder(
          borderSide: BorderSide(color: AppColors.primaryColor),
        ),
        focusedBorder: UnderlineInputBorder(
          borderSide: BorderSide(color: AppColors.tertiaryColor, width: 2),
        ),
        prefixIcon: Icon(Icons.person, color: Color(0xFF7F8C8D)),
      ),
      style: const TextStyle(color: Color(0xFF2C3E50)),
      cursorColor: AppColors.tertiaryColor,
      onFieldSubmitted: (_) {
        // Move focus to password field when Enter is pressed
        passwordFocusNode.requestFocus();
      },
    );
  }

  // Password field widget builder
  Widget _buildPasswordField() {
    return TextFormField(
      controller: _authModel.passwordController,
      focusNode: passwordFocusNode,
      decoration: InputDecoration(
        labelText: 'Contraseña',
        labelStyle: const TextStyle(color: AppColors.primaryColorGray),
        enabledBorder: const UnderlineInputBorder(
          borderSide: BorderSide(color: AppColors.primaryColor),
        ),
        focusedBorder: const UnderlineInputBorder(
          borderSide: BorderSide(color: AppColors.tertiaryColor, width: 2),
        ),
        prefixIcon: const Icon(Icons.lock, color: Color(0xFF7F8C8D)),
        suffixIcon: IconButton(
          icon: Icon(
            _isPasswordVisible ? Icons.visibility : Icons.visibility_off,
            color: const Color(0xFF7F8C8D),
          ),
          onPressed: () {
            setState(() {
              _isPasswordVisible = !_isPasswordVisible;
            });
          },
          tooltip: _isPasswordVisible ? 'Ocultar contraseña' : 'Mostrar contraseña',
        ),
      ),
      style: const TextStyle(color: Color(0xFF2C3E50)),
      cursorColor: AppColors.tertiaryColor,
      obscureText: !_isPasswordVisible,
      onFieldSubmitted: (_) {
        _fakeLogin();
      },
    );
  }

  // Login button widget builder
  Widget _buildLoginButton() {
    return _loading
        ? const CircularProgressIndicator(
            valueColor: AlwaysStoppedAnimation<Color>(AppColors.primaryColor),
          )
        : MouseRegion(
            cursor: SystemMouseCursors.click,
            onEnter: (_) => setState(() => _isHovering = true),
            onExit: (_) => setState(() => _isHovering = false),
            child: SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: _fakeLogin,
                style: ElevatedButton.styleFrom(
                  backgroundColor: _isHovering
                      ? AppColors.tertiaryColor
                      : AppColors.primaryColor,
                  foregroundColor: Colors.white,
                  padding:
                      const EdgeInsets.symmetric(horizontal: 30, vertical: 15),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                  elevation: 3,
                  shadowColor: AppColors.primaryColor,
                ),
                child: const Text(
                  'Iniciar sesión',
                  style: TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 16,
                  ),
                ),
              ),
            ),
          );
  }

  @override
  void dispose() {
    // Clean up the focus node when the Form is disposed.
    passwordFocusNode.dispose();
    _authModel.dispose();
    super.dispose();
  }
}
