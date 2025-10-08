import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:get/get.dart';
import 'package:vyaa_central_infor_webflutter/core/layouts/home.layout.dart';
import 'package:vyaa_central_infor_webflutter/core/shared/app_sidebar.widget.dart';
import 'package:vyaa_central_infor_webflutter/core/shared/app_table.widget.dart';
import 'package:vyaa_central_infor_webflutter/core/shared/app_form.widget.dart';
import 'package:vyaa_central_infor_webflutter/core/shared/app_button.widget.dart';
import 'package:vyaa_central_infor_webflutter/controllers/home_screen.getx.dart';
import 'package:vyaa_central_infor_webflutter/models/responses/quotation_res.module.dart';
import 'package:vyaa_central_infor_webflutter/models/requests/quotation_req.module.dart';
import 'package:vyaa_central_infor_webflutter/models/responses/customer_res.module.dart';
import 'package:vyaa_central_infor_webflutter/services/api/customer.service.dart';
import 'package:vyaa_central_infor_webflutter/services/local/cache_service.service.dart';


class HomePage extends StatelessWidget {
  HomePage({super.key});

  // Instancia del servicio de clientes
  final CustomerService _customerService = CustomerService();

  @override
  Widget build(BuildContext context) {
    final HomeScreenController controller = Get.put(HomeScreenController());

    return AppSidebar(
      sidebarItems: [
        SidebarItem(
          name: 'Home',
          icon: Icons.dashboard,
          onPress: () {
            // Acción para Home
            print('Home pressed');
          },
        ),
        SidebarItem(
          name: 'Refrescar',
          icon: Icons.refresh,
          onPress: () => controller.refreshData(),
        ),
      ],
      userAvatarTooltip: 'John Smith - Administrador',
      showLogout: true,
      body: Container(
        color: Colors.grey[200],
        child: HomeLayout(
          backgroundColor: Colors.grey[200],
          borderWidth: 0,

          // Columna izquierda - Formulario de creación
          leftColumn: _buildQuotationForm(controller),

          // Header derecho - Panel de control con estadísticas
          rightHeader: _buildStatsPanel(controller),

          // Body derecho - Tabla de cotizaciones
          rightBody: _buildQuotationsTable(controller),
        ),
      ),
    );
  }

  Widget _buildQuotationForm(HomeScreenController controller) {
    return Card(
      color: Colors.white,
      elevation: 2,
      margin: const EdgeInsets.all(16),
      child: Obx(() => AppForm(
        padding: const EdgeInsets.all(20),
        sections: [
          FormSection(
            title: 'Nueva Cotización',
            fields: [
              FormFieldConfig(
                label: 'Folio',
                type: FormFieldType.number,
                controller: controller.currentQuotation.value.folioController,
                isRequired: true,
                keyboardType: TextInputType.number,
                inputFormatters: [
                  FilteringTextInputFormatter.digitsOnly,
                ],
              ),
              FormFieldConfig(
                label: 'Fecha de Venta',
                type: FormFieldType.date,
                controller: TextEditingController(text: _formatDate(controller.currentQuotation.value.saleDate)),
                isRequired: true,
                onDateChanged: (date) {
                  if (date != null) {
                    controller.currentQuotation.value = QuotationReq.fromComponents(
                      generalCommentController: controller.currentQuotation.value.generalCommentController,
                      folioController: controller.currentQuotation.value.folioController,
                      salesExecutiveControllers: controller.currentQuotation.value.salesExecutiveControllers,
                      followups: controller.currentQuotation.value.followups,
                      customer: controller.currentQuotation.value.customer,
                      saleDate: date,
                    );
                  }
                },
              ),
              FormFieldConfig(
                label: 'Comentario General',
                type: FormFieldType.textarea,
                controller: controller.currentQuotation.value.generalCommentController,
                maxLines: 3,
              ),
            ],
          ),
          FormSection(
            title: 'Información del Cliente',
            isCollapsible: true,
            headerWidget: Obx(() {
              final hasCustomerId = controller.currentQuotation.value.customer.customerIdController.text.isNotEmpty;
              return hasCustomerId 
                ? Container(
                    margin: const EdgeInsets.only(bottom: 8),
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                    decoration: BoxDecoration(
                      color: Colors.green.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(color: Colors.green.withOpacity(0.3)),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.check_circle, color: Colors.green.shade600, size: 16),
                        const SizedBox(width: 6),
                        Text(
                          'Existente - ID: ${controller.currentQuotation.value.customer.customerIdController.text}',
                          style: TextStyle(
                            color: Colors.green.shade700,
                            fontSize: 12,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        const SizedBox(width: 8),
                        GestureDetector(
                          onTap: () {
                            // Limpiar selección de cliente existente
                            controller.currentQuotation.value.customer.customerIdController.clear();
                            controller.currentQuotation.value.customer.fullNameController.clear();
                            controller.currentQuotation.value.customer.codeController.clear();
                            controller.currentQuotation.value.customer.emailController.clear();
                            controller.currentQuotation.value.customer.phoneController.clear();
                            controller.currentQuotation.refresh();
                          },
                          child: Icon(Icons.close, color: Colors.green.shade600, size: 16),
                        ),
                      ],
                    ),
                  )
                : Container(
                    margin: const EdgeInsets.only(bottom: 8),
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                    decoration: BoxDecoration(
                      color: Colors.blue.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(color: Colors.blue.withOpacity(0.3)),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.person_add, color: Colors.blue.shade600, size: 16),
                        const SizedBox(width: 6),
                        Text(
                          'Creando nuevo cliente',
                          style: TextStyle(
                            color: Colors.blue.shade700,
                            fontSize: 12,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                      ],
                    ),
                  );
            }),
            fields: [
              // Campo de búsqueda de cliente con sugerencias
              FormFieldConfig(
                label: 'Buscar Cliente Existente',
                type: FormFieldType.suggest,
                controller: TextEditingController(), // Controlador temporal para búsqueda
                hint: 'Escriba el nombre del cliente para buscar...',
                onSearch: (query) async {
                  if (query.length < 2) {
                    return <CustomerRes>[];
                  }
                  
                  final response = await _customerService.searchCustomers(name: query);
                  if (response.isSuccess && response.data != null) {
                    return response.data!;
                  }
                  return <CustomerRes>[];
                },
                itemToString: (customer) {
                  final customerRes = customer as CustomerRes;
                  return '${customerRes.fullName} - ${customerRes.code}';
                },
                onItemSelected: (selectedCustomer) {
                  final customer = selectedCustomer as CustomerRes;
                  // Setear el ID del cliente (esto indica que es cliente existente)
                  controller.currentQuotation.value.customer.customerIdController.text = customer.customerId;
                  // Llenar todos los campos del cliente
                  controller.currentQuotation.value.customer.fullNameController.text = customer.fullName;
                  controller.currentQuotation.value.customer.codeController.text = customer.code;
                  controller.currentQuotation.value.customer.emailController.text = customer.email;
                  controller.currentQuotation.value.customer.phoneController.text = 
                    customer.phones.isNotEmpty ? customer.phones.first : '';
                  
                  // Refrescar la vista
                  controller.currentQuotation.refresh();
                },
              ),
              // Campos editables del cliente
              FormFieldConfig(
                label: 'Nombre Completo',
                controller: controller.currentQuotation.value.customer.fullNameController,
                isRequired: true,
                hint: 'Nombre del cliente',
              ),
              FormFieldConfig(
                label: 'Código',
                controller: controller.currentQuotation.value.customer.codeController,
                hint: 'Código del cliente',
              ),
              FormFieldConfig(
                label: 'Teléfono',
                controller: controller.currentQuotation.value.customer.phoneController,
                hint: 'Teléfono del cliente',
              ),
              FormFieldConfig(
                label: 'Email',
                type: FormFieldType.email,
                controller: controller.currentQuotation.value.customer.emailController,
                hint: 'Email del cliente',
              ),
            ],
          ),
          FormSection(
            title: 'Seguimientos',
            isCollapsible: true,
            initiallyExpanded: false,
            headerWidget: Padding(
              padding: const EdgeInsets.symmetric(vertical: 8.0),
              child: Row(
                children: [
                  const Expanded(
                    child: Text(
                      'Agregar seguimientos a la cotización',
                      style: TextStyle(fontSize: 14, color: Colors.grey),
                    ),
                  ),
                  AppButton(
                    label: 'Agregar Seguimiento',
                    size: 'small',
                    colorType: 'primary',
                    onPressed: () async {
                      // Obtener el userId del cache
                      final userId = await CacheService.getUserId();
                      
                      // Agregar el seguimiento
                      controller.currentQuotation.value.addFollowup();
                      
                      // Establecer automáticamente el userId en el nuevo seguimiento
                      final followups = controller.currentQuotation.value.followups;
                      if (followups.isNotEmpty && userId != null) {
                        followups.last.userIdController.text = userId;
                      }
                      
                      controller.currentQuotation.refresh();
                    },
                  ),
                ],
              ),
            ),
            fields: _buildFollowupFields(controller),
          ),
        ],
        footer: Column(
          children: [
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: AppButton(
                    label: controller.isCreating.value ? 'Creando...' : 'Crear Cotización',
                    colorType: 'success',
                    onPressed: controller.isCreating.value ? null : () {
                      controller.createQuotation();
                    },
                  ),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: AppButton(
                    label: 'Limpiar',
                    colorType: 'secondary',
                    onPressed: () => controller.resetForm(),
                  ),
                ),
              ],
            ),
          ],
        ),
      )),
    );
  }

  Widget _buildStatsPanel(HomeScreenController controller) {
    return Card(
      color: Colors.white,
      elevation: 2,
      margin: const EdgeInsets.all(16),
      child: Container(
        width: double.infinity,
        padding: const EdgeInsets.all(12),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.construction,
              size: 24,
              color: Colors.grey.withOpacity(0.6),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildQuotationsTable(HomeScreenController controller) {
    return Card(
      color: Colors.white,
      elevation: 2,
      margin: const EdgeInsets.all(16),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Cotizaciones',
                  style: Theme.of(Get.context!).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                IconButton(
                  icon: const Icon(Icons.refresh),
                  onPressed: () => controller.refreshData(),
                ),
              ],
            ),
            const SizedBox(height: 16),
            Expanded(
              child: Obx(() {
                if (controller.isLoading.value) {
                  return const Center(
                    child: CircularProgressIndicator(),
                  );
                }

                if (controller.quotations.isEmpty) {
                  return const Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          Icons.inbox_outlined,
                          size: 64,
                          color: Colors.grey,
                        ),
                        SizedBox(height: 16),
                        Text(
                          'No hay cotizaciones disponibles',
                          style: TextStyle(
                            fontSize: 16,
                            color: Colors.grey,
                          ),
                        ),
                      ],
                    ),
                  );
                }

                return GenericTable<QuotationRes>(
                  items: controller.quotations,
                  columns: [
                    TableColumn<QuotationRes>(
                      title: 'Folio',
                      builder: (item) => Text(item.folio.toString()),
                    ),
                    TableColumn<QuotationRes>(
                      title: 'Cliente',
                      builder: (item) => Text(item.customer.fullName),
                    ),
                    TableColumn<QuotationRes>(
                      title: 'Fecha',
                      builder: (item) => Text(_formatDate(item.saleDate)),
                    ),
                    TableColumn<QuotationRes>(
                      title: 'Estado',
                      builder: (item) => Chip(
                        label: Text(
                          item.status,
                          style: const TextStyle(fontSize: 12),
                        ),
                        backgroundColor: _getStatusColor(item.status),
                      ),
                    ),
                    TableColumn<QuotationRes>(
                      title: 'Comentario',
                      builder: (item) => Text(
                        item.generalComment.isNotEmpty 
                            ? item.generalComment 
                            : 'Sin comentarios',
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                  ],
                  actionButtonsBuilder: (item) => TableActionButtons<QuotationRes>(
                    item: item,
                    onEditPressed: () {
                      // TODO: Implementar edición
                      print('Editar cotización: ${item.quotationId}');
                    },
                    onDeletePressed: () {
                      // TODO: Implementar eliminación
                      print('Eliminar cotización: ${item.quotationId}');
                    },
                  ),
                  topActions: [
                    AppButton(
                      label: 'Filtrar',
                      size: 'small',
                      colorType: 'info',
                      onPressed: () {
                        // TODO: Implementar filtros
                        print('Filtrar cotizaciones');
                      },
                    ),
                  ],
                );
              }),
            ),
          ],
        ),
      ),
    );
  }

  Color _getStatusColor(String status) {
    final statusLower = status.toLowerCase();
    if (statusLower.contains('pending') || statusLower.contains('pendiente')) {
      return Colors.orange.withOpacity(0.2);
    } else if (statusLower.contains('approved') || statusLower.contains('aprobado')) {
      return Colors.green.withOpacity(0.2);
    } else if (statusLower.contains('rejected') || statusLower.contains('rechazado')) {
      return Colors.red.withOpacity(0.2);
    }
    return Colors.grey.withOpacity(0.2);
  }

  String _formatDate(DateTime date) {
    return '${date.day.toString().padLeft(2, '0')}/${date.month.toString().padLeft(2, '0')}/${date.year}';
  }

  List<FormFieldConfig> _buildFollowupFields(HomeScreenController controller) {
    final followups = controller.currentQuotation.value.followups;
    final List<FormFieldConfig> fields = [];

    if (followups.isEmpty) {
      fields.add(
        FormFieldConfig(
          label: 'Sin seguimientos',
          type: FormFieldType.text,
          controller: TextEditingController(text: 'No hay seguimientos agregados. Usa el botón "Agregar Seguimiento" para crear uno.'),
          enabled: false,
        ),
      );
      return fields;
    }

    for (int i = 0; i < followups.length; i++) {
      final followup = followups[i];
      
      // Campo de comentario
      fields.add(
        FormFieldConfig(
          label: 'Comentario del Seguimiento ${i + 1}',
          type: FormFieldType.textarea,
          controller: followup.commentController,
          maxLines: 2,
          isRequired: true,
          suffixIcon: followups.length > 1 ? IconButton(
            icon: const Icon(Icons.delete, color: Colors.red),
            onPressed: () {
              controller.currentQuotation.value.removeFollowup(i);
              controller.currentQuotation.refresh();
            },
            tooltip: 'Eliminar seguimiento',
          ) : null,
        ),
      );

      // Campo de Usuario ID - Solo lectura con valor del cache
      fields.add(
        FormFieldConfig(
          label: 'Usuario Responsable ${i + 1}',
          controller: followup.userIdController,
          enabled: false, // No editable
          hint: 'Usuario actual (automático)',
          prefixIcon: const Icon(Icons.person, color: Colors.grey),
        ),
      );
    }

    return fields;
  }
}
