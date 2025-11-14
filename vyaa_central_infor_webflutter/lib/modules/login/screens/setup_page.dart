import 'dart:ui';

import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../controllers/setup_page.getx.dart';

class SetupPage extends StatelessWidget {
  const SetupPage({super.key});

  @override
  Widget build(BuildContext context) {
    // Inicializar el controlador
    final controller = Get.put(SetupController());
    
    return Scaffold(
      backgroundColor: Colors.grey[50],
      body: Row(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          // Columna izquierda - Información e instrucciones
          Expanded(flex: 4, child: _buildInfoColumn(controller)),

          const SizedBox(width: 16),

          // Columna derecha - Formulario de configuración
          Expanded(flex: 6, child: _buildFormColumn()),
        ],
      ),
    );
  }

  Widget _buildInfoColumn(SetupController controller) {
    return Container(
      decoration: BoxDecoration(
        image: DecorationImage(
          image: AssetImage('assets/backgrounds/background-setup.jpg'),
          fit: BoxFit.cover,
        ),
      ),
      child: BackdropFilter(
        filter: ImageFilter.blur(sigmaX: 3.0, sigmaY: 3.0),
        child: Padding(
          padding: const EdgeInsets.all(48.0),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              // CARD de información con datos del cache
              Obx(() => Container(
                width: double.infinity,
                padding: const EdgeInsets.all(24),
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(12),
                  gradient: LinearGradient(
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                    colors: controller.isLoading.value 
                      ? [
                          Colors.grey.shade600,
                          Colors.grey.shade400,
                        ]
                      : [
                          const Color(0xFF1E3A8A), // Azul marino oscuro
                          const Color(0xFF3B82F6), // Azul más claro
                        ],
                  ),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.blue.withOpacity(0.3),
                      blurRadius: 12,
                      offset: const Offset(0, 4),
                    ),
                  ],
                ),
                child: controller.isLoading.value
                  ? const Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        CircularProgressIndicator(color: Colors.white),
                        SizedBox(height: 16),
                        Text(
                          'Cargando datos del sistema...',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                          ),
                          textAlign: TextAlign.center,
                        ),
                      ],
                    )
                  : Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        // Icono dinámico basado en estado
                        Container(
                          padding: const EdgeInsets.all(16),
                          decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.2),
                            shape: BoxShape.circle,
                          ),
                          child: Icon(
                            controller.getStatusIcon(),
                            size: 48,
                            color: Colors.white,
                          ),
                        ),

                        const SizedBox(height: 16),

                        // Título
                        const Text(
                          'Estado del Sistema',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                          ),
                          textAlign: TextAlign.center,
                        ),

                        const SizedBox(height: 8),

                        // Mensaje de estado dinámico
                        Text(
                          controller.statusMessage.value,
                          style: const TextStyle(color: Colors.white70, fontSize: 14),
                          textAlign: TextAlign.center,
                        ),

                        const SizedBox(height: 16),

                        // Datos directos con información de Isar en tiempo real
                        Container(
                          width: double.infinity,
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.1),
                            borderRadius: BorderRadius.circular(8),
                            border: Border.all(
                              color: Colors.white.withOpacity(0.2),
                              width: 1,
                            ),
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                children: [
                                  Icon(Icons.storage, color: Colors.white70, size: 16),
                                  const SizedBox(width: 8),
                                  const Text(
                                    'Base de Datos Hive',
                                    style: TextStyle(
                                      color: Colors.white,
                                      fontWeight: FontWeight.bold,
                                      fontSize: 12,
                                    ),
                                  ),
                                  const Spacer(),
                                  Container(
                                    padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                                    decoration: BoxDecoration(
                                      color: Colors.green.withOpacity(0.3),
                                      borderRadius: BorderRadius.circular(4),
                                    ),
                                    child: const Text(
                                      'DATOS CRUDOS',
                                      style: TextStyle(
                                        color: Colors.white,
                                        fontSize: 8,
                                        fontWeight: FontWeight.bold,
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 8),
                              // Información de configuraciones
                              Obx(() => Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: [
                                      Text(
                                        'Primera vez: ${controller.isFirstTimeCheck.value ? 'Sí' : 'No'}',
                                        style: const TextStyle(color: Colors.white70, fontSize: 10),
                                      ),
                                      Text(
                                        'Inicializado: ${controller.isSystemInitialized.value ? 'Sí' : 'No'}',
                                        style: const TextStyle(color: Colors.white70, fontSize: 10),
                                      ),
                                    ],
                                  ),
                                  const SizedBox(height: 4),
                                  Text(
                                    'Ruta: ${controller.lastRoute.value}',
                                    style: const TextStyle(color: Colors.white70, fontSize: 10),
                                  ),
                                  Text(
                                    'Estadísticas BD: ${controller.databaseStats.length} cajas',
                                    style: const TextStyle(color: Colors.white70, fontSize: 10),
                                  ),
                                  const SizedBox(height: 8),
                                  Container(
                                    constraints: const BoxConstraints(maxHeight: 120),
                                    child: SingleChildScrollView(
                                      child: Text(
                                        controller.getRawDataForDisplay(),
                                        style: const TextStyle(
                                          color: Colors.white70,
                                          fontSize: 8,
                                          fontFamily: 'monospace',
                                        ),
                                      ),
                                    ),
                                  ),
                                ],
                              )),
                            ],
                          ),
                        ),

                        const SizedBox(height: 12),

                        // Botón de refrescar
                        TextButton.icon(
                          onPressed: () => controller.refreshData(),
                          icon: const Icon(Icons.refresh, color: Colors.white70, size: 16),
                          label: const Text(
                            'Actualizar estado',
                            style: TextStyle(color: Colors.white70, fontSize: 12),
                          ),
                        ),
                      ],
                    ),
              )),

              const SizedBox(height: 20),

              // Espacio para futuros avisos
              Expanded(
                child: Container(
                  decoration: BoxDecoration(
                    color: Colors.white.withOpacity(0.9),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.grey[300]!),
                  ),
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Icon(Icons.info_outline, 
                            color: Colors.grey[600], size: 20),
                          const SizedBox(width: 8),
                          Text(
                            'Información del Sistema',
                            style: TextStyle(
                              fontWeight: FontWeight.bold,
                              color: Colors.grey[700],
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),
                      Expanded(
                        child: SingleChildScrollView(
                          child: Obx(() => Text(
                            controller.isLoading.value
                              ? 'Cargando información...'
                              : 'Sistema: ${controller.isSystemInitialized.value ? "Configurado" : "Pendiente"}\n'
                                'Estado: ${controller.statusMessage.value}\n'
                                'Configuración inicial necesaria: ${controller.isFirstTimeCheck.value ? "Sí" : "No"}',
                            style: TextStyle(
                              fontSize: 12, 
                              color: Colors.grey[600],
                              height: 1.4,
                            ),
                          )),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }



  Widget _buildFormColumn() {
    return Container(
      color: Colors.white,
      padding: const EdgeInsets.all(24),
      child: GetBuilder<SetupController>(
        builder: (controller) => Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header dinámico del formulario
            Obx(() => Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  controller.getCurrentStepTitle(),
                  style: const TextStyle(
                    fontSize: 24,
                    fontWeight: FontWeight.bold,
                    color: Colors.blueGrey,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  controller.getCurrentStepDescription(),
                  style: const TextStyle(color: Colors.grey),
                ),
              ],
            )),

            const SizedBox(height: 24),

            // Indicador de progreso
            Obx(() => _buildProgressIndicator(controller)),

            const SizedBox(height: 32),

            // Estado del formulario
            Obx(() => controller.formStatus.value.isNotEmpty
                ? Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(16),
                    margin: const EdgeInsets.only(bottom: 16),
                    decoration: BoxDecoration(
                      color: controller.isFormLoading.value 
                        ? Colors.blue[50] 
                        : Colors.green[50],
                      borderRadius: BorderRadius.circular(8),
                      border: Border.all(
                        color: controller.isFormLoading.value 
                          ? Colors.blue[200]! 
                          : Colors.green[200]!,
                      ),
                    ),
                    child: Row(
                      children: [
                        controller.isFormLoading.value
                          ? const SizedBox(
                              width: 16,
                              height: 16,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : Icon(
                              Icons.info_outline,
                              color: Colors.green[700],
                              size: 20,
                            ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Text(
                            controller.formStatus.value,
                            style: TextStyle(
                              color: controller.isFormLoading.value 
                                ? Colors.blue[700] 
                                : Colors.green[700],
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                        ),
                      ],
                    ),
                  )
                : const SizedBox.shrink()),

            // Contenido del formulario dinámico
            Expanded(
              child: Obx(() => _buildFormContent(controller)),
            ),

            const SizedBox(height: 16),

            // Botones de acción dinámicos
            Obx(() => _buildActionButtons(controller)),
          ],
        ),
      ),
    );
  }

  Widget _buildProgressIndicator(SetupController controller) {
    final steps = ['Migraciones', 'Sistema', 'Administrador', 'Completado'];
    final currentStep = controller.currentStep.value;

    return Row(
      children: List.generate(steps.length, (index) {
        final isCompleted = index < currentStep;
        final isCurrent = index == currentStep;
        final isActive = isCompleted || isCurrent;

        return Expanded(
          child: Row(
            children: [
              // Círculo del paso
              Container(
                width: 30,
                height: 30,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: isCompleted
                      ? Colors.green
                      : isCurrent
                          ? Colors.blue
                          : Colors.grey[300],
                ),
                child: Center(
                  child: isCompleted
                      ? const Icon(Icons.check, color: Colors.white, size: 16)
                      : Text(
                          '${index + 1}',
                          style: TextStyle(
                            color: isActive ? Colors.white : Colors.grey[600],
                            fontSize: 12,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                ),
              ),
              
              // Línea conectora (excepto el último)
              if (index < steps.length - 1)
                Expanded(
                  child: Container(
                    height: 2,
                    margin: const EdgeInsets.symmetric(horizontal: 8),
                    color: isCompleted ? Colors.green : Colors.grey[300],
                  ),
                ),
            ],
          ),
        );
      }),
    );
  }

  Widget _buildFormContent(SetupController controller) {
    switch (controller.currentStep.value) {
      case 0: // Migraciones
        return _buildMigrationsStep(controller);
      case 1: // Configuración del sistema
        return _buildSystemConfigStep(controller);
      case 2: // Configuración del administrador
        return _buildAdminConfigStep(controller);
      case 3: // Completado
        return _buildCompletedStep();
      default:
        return _buildLoadingStep();
    }
  }

  Widget _buildMigrationsStep(SetupController controller) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(
            Icons.data_usage_outlined,
            size: 64,
            color: Colors.blue,
          ),
          const SizedBox(height: 24),
          const Text(
            'Aplicar Migraciones de Base de Datos',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.bold,
              color: Colors.blueGrey,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 16),
          const Text(
            'El sistema necesita aplicar migraciones a la base de datos '
            'para crear las tablas y estructuras necesarias.',
            style: TextStyle(color: Colors.grey),
            textAlign: TextAlign.center,
          ),
          
          const SizedBox(height: 24),
          
          // Debug info para mostrar por qué estamos aquí
          Obx(() => controller.systemConfig.value != null
            ? Container(
                padding: const EdgeInsets.all(16),
                margin: const EdgeInsets.symmetric(horizontal: 32),
                decoration: BoxDecoration(
                  color: Colors.red[50],
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.red[200]!),
                ),
                child: Column(
                  children: [
                    const Text(
                      'Estado Actual del Sistema:',
                      style: TextStyle(
                        fontWeight: FontWeight.bold,
                        fontSize: 12,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'requiresMigration: ${controller.systemConfig.value!.requiresMigration}',
                      style: const TextStyle(fontSize: 11, fontFamily: 'monospace'),
                    ),
                    Text(
                      'hasConfiguration: ${controller.systemConfig.value!.hasConfiguration}',
                      style: const TextStyle(fontSize: 11, fontFamily: 'monospace'),
                    ),
                    Text(
                      'tableExists: ${controller.systemConfig.value!.tableExists}',
                      style: const TextStyle(fontSize: 11, fontFamily: 'monospace'),
                    ),
                    const SizedBox(height: 8),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                      children: [
                        ElevatedButton.icon(
                          onPressed: () => controller.checkSystemStatus(),
                          icon: const Icon(Icons.refresh, size: 16),
                          label: const Text('Verificar'),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.orange,
                            foregroundColor: Colors.white,
                            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                          ),
                        ),
                        ElevatedButton.icon(
                          onPressed: () => controller.forceGoToConfiguration(),
                          icon: const Icon(Icons.skip_next, size: 16),
                          label: const Text('Omitir'),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.grey[600],
                            foregroundColor: Colors.white,
                            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              )
            : const SizedBox.shrink(),
          ),
        ],
      ),
    );
  }

  Widget _buildSystemConfigStep(SetupController controller) {
    return SingleChildScrollView(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Información de la Empresa',
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.bold,
              color: Colors.blueGrey,
            ),
          ),
          const SizedBox(height: 16),
          
          TextFormField(
            controller: controller.configSysReq.companyNameController,
            decoration: const InputDecoration(
              labelText: 'Nombre de la Empresa',
              hintText: 'Ej: Mi Empresa S.A.',
              border: OutlineInputBorder(),
              prefixIcon: Icon(Icons.business),
            ),
          ),
          const SizedBox(height: 16),
          
          TextFormField(
            controller: controller.configSysReq.branchNameController,
            decoration: const InputDecoration(
              labelText: 'Nombre de la Sucursal',
              hintText: 'Ej: Sucursal Central',
              border: OutlineInputBorder(),
              prefixIcon: Icon(Icons.store),
            ),
          ),
          const SizedBox(height: 16),
          
          TextFormField(
            controller: controller.configSysReq.logoUrlController,
            decoration: const InputDecoration(
              labelText: 'URL del Logo (Opcional)',
              hintText: 'https://ejemplo.com/logo.png',
              border: OutlineInputBorder(),
              prefixIcon: Icon(Icons.image),
            ),
          ),
          
          const SizedBox(height: 16),
          
          // Debug info
          Obx(() => Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.blue[50],
              borderRadius: BorderRadius.circular(8),
              border: Border.all(color: Colors.blue[200]!),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Estado de Validación:',
                      style: TextStyle(fontWeight: FontWeight.bold, fontSize: 12),
                    ),
                    TextButton(
                      onPressed: () => controller.checkSystemStatus(),
                      style: TextButton.styleFrom(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                        minimumSize: Size.zero,
                      ),
                      child: const Text(
                        'Refrescar Estado',
                        style: TextStyle(fontSize: 10),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 4),
                Text(
                  'Empresa: "${controller.configSysReq.companyNameController.text}"',
                  style: const TextStyle(fontSize: 10, fontFamily: 'monospace'),
                ),
                Text(
                  'Sucursal: "${controller.configSysReq.branchNameController.text}"',
                  style: const TextStyle(fontSize: 10, fontFamily: 'monospace'),
                ),
                Text(
                  'Formulario válido: ${controller.isSystemFormValid.value}',
                  style: TextStyle(
                    fontSize: 10, 
                    fontFamily: 'monospace',
                    color: controller.isSystemFormValid.value ? Colors.green : Colors.red,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                if (controller.systemConfig.value != null) ...[
                  const SizedBox(height: 4),
                  Text(
                    'Sistema - hasConfig: ${controller.systemConfig.value!.hasConfiguration}',
                    style: const TextStyle(fontSize: 9, fontFamily: 'monospace', color: Colors.grey),
                  ),
                  Text(
                    'Sistema - requiresMigration: ${controller.systemConfig.value!.requiresMigration}',
                    style: const TextStyle(fontSize: 9, fontFamily: 'monospace', color: Colors.grey),
                  ),
                  Text(
                    'Paso actual: ${controller.currentStep.value}',
                    style: const TextStyle(fontSize: 9, fontFamily: 'monospace', color: Colors.grey),
                  ),
                ],
              ],
            ),
          )),
        ],
      ),
    );
  }

  Widget _buildAdminConfigStep(SetupController controller) {
    return SingleChildScrollView(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Usuario Administrador',
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.bold,
              color: Colors.blueGrey,
            ),
          ),
          const SizedBox(height: 16),
          
          TextFormField(
            controller: controller.configAdminReq.userNameController,
            decoration: const InputDecoration(
              labelText: 'Nombre de Usuario',
              hintText: 'admin',
              border: OutlineInputBorder(),
              prefixIcon: Icon(Icons.person),
            ),
          ),
          const SizedBox(height: 16),
          
          TextFormField(
            controller: controller.configAdminReq.passwordController,
            obscureText: true,
            decoration: const InputDecoration(
              labelText: 'Contraseña',
              hintText: 'Mínimo 6 caracteres',
              border: OutlineInputBorder(),
              prefixIcon: Icon(Icons.lock),
            ),
          ),
          const SizedBox(height: 16),
          
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.amber[50],
              borderRadius: BorderRadius.circular(8),
              border: Border.all(color: Colors.amber[200]!),
            ),
            child: Row(
              children: [
                const Icon(Icons.security, color: Colors.amber),
                const SizedBox(width: 12),
                const Expanded(
                  child: Text(
                    'Guarde estas credenciales en un lugar seguro. '
                    'Las necesitará para acceder al sistema.',
                    style: TextStyle(fontSize: 12),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCompletedStep() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(
            Icons.check_circle_outline,
            size: 64,
            color: Colors.green,
          ),
          const SizedBox(height: 24),
          const Text(
            '¡Configuración Completada!',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.bold,
              color: Colors.green,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 16),
          const Text(
            'El sistema ha sido configurado exitosamente. '
            'Será redirigido al login automáticamente.',
            style: TextStyle(color: Colors.grey),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }

  Widget _buildLoadingStep() {
    return const Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          CircularProgressIndicator(),
          SizedBox(height: 16),
          Text('Cargando...'),
        ],
      ),
    );
  }

  Widget _buildActionButtons(SetupController controller) {
    switch (controller.currentStep.value) {
      case 0: // Migraciones
        return SizedBox(
          width: double.infinity,
          child: ElevatedButton.icon(
            onPressed: controller.isFormLoading.value 
              ? null 
              : () => controller.applyMigrations(),
            icon: const Icon(Icons.play_arrow),
            label: const Text('Aplicar Migraciones'),
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
            ),
          ),
        );
        
      case 1: // Sistema
        return Obx(() => SizedBox(
          width: double.infinity,
          child: ElevatedButton.icon(
            onPressed: controller.isFormLoading.value || !controller.isSystemFormValid.value
              ? null 
              : () => controller.configureSystem(),
            icon: const Icon(Icons.settings),
            label: const Text('Configurar Sistema'),
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
            ),
          ),
        ));
        
      case 2: // Admin
        return Obx(() => SizedBox(
          width: double.infinity,
          child: ElevatedButton.icon(
            onPressed: controller.isFormLoading.value || !controller.isAdminFormValid.value
              ? null 
              : () => controller.configureAdmin(),
            icon: const Icon(Icons.admin_panel_settings),
            label: const Text('Crear Administrador'),
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
            ),
          ),
        ));
        
      case 3: // Completado
        return SizedBox(
          width: double.infinity,
          child: ElevatedButton.icon(
            onPressed: () => Get.offAllNamed('/login'),
            icon: const Icon(Icons.login),
            label: const Text('Ir al Login'),
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
            ),
          ),
        );
        
      default:
        return const SizedBox.shrink();
    }
  }
}

class CreateMigration extends StatelessWidget {
  const CreateMigration({super.key});

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(child: Text('Create Migration Page - Under Construction')),
    );
  }
}

class ConfigurationForm extends StatelessWidget {
  const ConfigurationForm({super.key});

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(child: Text('Configuration Form - Under Construction')),
    );
  }
}
