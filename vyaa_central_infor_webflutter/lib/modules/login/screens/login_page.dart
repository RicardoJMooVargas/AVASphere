import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:vyaa_central_infor_webflutter/core/core.dart';
import 'dart:ui';
import 'package:vyaa_central_infor_webflutter/core/controllers/notification_services.dart';
import '../services/api/auth.service.dart';
import '../models/auth_req.module.dart';
import '../../../core/layouts/login_page.layout.dart';
import '../../login/widgets/login_header.widget.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final AuthReq _authModel = AuthReq();
  bool _loading = false;
  final FocusNode _passwordFocusNode = FocusNode();
  final GlobalKey<FormState> _formKey = GlobalKey<FormState>();

  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _loading = true);
    try {
      final service = AuthService();
      final response = await service.login(_authModel);

      if (response.success) {
        // Navegar al home directamente sin mostrar notificación
        debugPrint('✅ Login exitoso - Navegando al dashboard');
        if (mounted) {
          context.go('/app/home');
        }
      } else {
        // Solo mostrar errores cuando falla el login
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Row(
                children: [
                  const Icon(Icons.error_outline, color: Colors.white),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(response.message ?? 'Error al iniciar sesión'),
                  ),
                ],
              ),
              backgroundColor: Colors.red.shade700,
              behavior: SnackBarBehavior.floating,
              duration: const Duration(seconds: 4),
            ),
          );
        }
      }
    } catch (e) {
      debugPrint('❌ Error en login: $e');
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Row(
              children: [
                const Icon(Icons.error_outline, color: Colors.white),
                const SizedBox(width: 8),
                Expanded(
                  child: Text('Error inesperado: ${e.toString()}'),
                ),
              ],
            ),
            backgroundColor: Colors.red.shade700,
            behavior: SnackBarBehavior.floating,
            duration: const Duration(seconds: 4),
          ),
        );
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        // Fondo
        _buildBackground(),
        // Layout de 2 columnas
        LoginPageLayout(
          leftColumn: _buildLeftColumn(),
          rightColumn: _buildRightColumn(),
        ),
      ],
    );
  }

  Widget _buildBackground() {
    return Positioned.fill(
      child: Transform.scale(
        scaleX: -1,
        child: ImageFiltered(
          imageFilter: ImageFilter.blur(sigmaX: 5, sigmaY: 5),
          child: Image.asset(
            'assets/backgrounds/background-login.jpg',
            fit: BoxFit.cover,
          ),
        ),
      ),
    );
  }

  Widget _buildLeftColumn() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(60.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
            children: [
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(6),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.15),
                    spreadRadius: 2,
                    blurRadius: 20,
                    offset: const Offset(0, 5),
                  ),
                ],
              ),
              child: Column(
                children: [
                  Icon(Icons.security, size: 80, color: AppColors.tertiaryColor),
                  const SizedBox(height: 10),
                  Text(
                    'Plataforma de gestión y administración integral para todos tus procesos empresariales',
                    style: TextStyle(color: Colors.black, fontSize: 14, decoration: TextDecoration.none),
                    textAlign: TextAlign.center,
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildRightColumn() {
    return Card(
        elevation: 8,
        color: Colors.transparent,
        margin: EdgeInsets.zero,
        child: ClipRRect(
          borderRadius: const BorderRadius.only(
            topLeft: Radius.circular(6),
            bottomLeft: Radius.circular(6),
          ),
          child: BackdropFilter(
            filter: ImageFilter.blur(sigmaX: 5, sigmaY: 5),
            child: Container(
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                  colors: [
                    Colors.grey.shade300.withOpacity(0.6),
                    Colors.white,
                  ],
                ),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.15),
                    spreadRadius: 2,
                    blurRadius: 20,
                    offset: const Offset(0, 5),
                  ),
                ],
              ),
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  horizontal: 50,
                  vertical: 40,
                ),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  crossAxisAlignment: CrossAxisAlignment.center,
                  children: [
                    // Header del login
                    LoginHeader(
                      title: 'AVASphere',
                      subtitle: 'Sistema de Autenticación Central',
                      icon: Icons.build,
                    ),
                    const SizedBox(height: 50),

                    // Formulario usando tu componente AppForm completo
                    AppForm(
                      formKey: _formKey,
                      sections: [
                        FormSection(
                          title: '',
                          fields: [
                            FormFieldConfig(
                              label: 'Usuario',
                              type: FormFieldType.text,
                              controller: _authModel.userNameController,
                              isRequired: true,
                              prefixIcon: Icon(
                                Icons.person_outline,
                                color: AppColors.primaryColor,
                              ),
                              validator: (value) {
                                if (value == null || value.trim().isEmpty) {
                                  return 'Por favor ingrese su usuario';
                                }
                                return null;
                              },
                            ),
                            FormFieldConfig(
                              label: 'Contraseña',
                              type: FormFieldType.password,
                              controller: _authModel.passwordController,
                              isRequired: true,
                              prefixIcon: Icon(
                                Icons.lock_outline,
                                color: AppColors.primaryColor,
                              ),
                              validator: (value) {
                                if (value == null || value.trim().isEmpty) {
                                  return 'Por favor ingrese su contraseña';
                                }
                                if (value.length < 3) {
                                  return 'La contraseña debe tener al menos 4 caracteres';
                                }
                                return null;
                              },
                            ),
                          ],
                        ),
                      ],
                      spacing: 25.0,
                      sectionSpacing: 0.0,
                      footer: _buildLoginButton(),
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      );
  }

  Widget _buildLoginButton() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        const SizedBox(height: 10),
        AppButton(
          label: 'Iniciar Sesión',
          colorType: 'primary',
          onPressed: _login,
          isLoading: _loading,
          fullWidth: true,
          size: 'large',
        ),
        const SizedBox(height: 20),
        // Enlace de ayuda
        GestureDetector(
          onTap: () {
            // Mostrar diálogo de ayuda sin usar GetX
            showDialog(
              context: context,
              builder: (context) => AlertDialog(
                title: const Text('Ayuda'),
                content: const Text('Funcionalidad de ayuda en desarrollo'),
                actions: [
                  TextButton(
                    onPressed: () => Navigator.of(context).pop(),
                    child: const Text('OK'),
                  ),
                ],
              ),
            );
          },
          child: Text(
            '¿Necesitas ayuda para acceder?',
            style: TextStyle(
              color: AppColors.primaryColor,
              fontSize: 14,
              fontWeight: FontWeight.w500,
            ),
            textAlign: TextAlign.center,
          ),
        ),
      ],
    );
  }

  @override
  void dispose() {
    _passwordFocusNode.dispose();
    _authModel.dispose();
    super.dispose();
  }
}
