import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../models/requests/quotation_req.module.dart';
import '../models/response/quotation_res.module.dart';
import '../../../Core/widgets/forms/app_form.widget.dart';
import '../../../Core/widgets/buttons/app_button.widget.dart';

import '../../../Core/models/responses/customer_res.module.dart';
import '../../../Core/services/api/customer.service.dart';

import '../../../Core/services/data/hive.service.dart';
import '../services/api/quotation_manager.service.dart';

import '../../../Core/theme/app_colors.dart';

class QuotationFormWidget extends StatefulWidget {
  final QuotationReq quotationModel;
  final QuotationRes? existingQuotation;
  final bool isUpdate;

  const QuotationFormWidget({
    super.key,
    required this.quotationModel,
    this.existingQuotation,
    this.isUpdate = false,
  });

  @override
  State<QuotationFormWidget> createState() => _QuotationFormWidgetState();
}

class _QuotationFormWidgetState extends State<QuotationFormWidget> {
  late final CustomerService _customerService;
  final QuotationManagerService _quotationService = const QuotationManagerService();
  late GlobalKey<FormState> _formKey;
  late TextEditingController _customerSearchController;

  @override
  void initState() {
    super.initState();
    _customerService = CustomerService();
    _formKey = GlobalKey<FormState>();
    _customerSearchController = TextEditingController();

    // Agregar el usuario actual como ejecutivo de ventas por defecto
    _initializeDefaultSalesExecutive();
  }

  /// Inicializa el ejecutivo de ventas por defecto con el usuario actual
  Future<void> _initializeDefaultSalesExecutive() async {
    try {
      final userNameCache = await HiveService.getCache('user.name');
      final userName = userNameCache ?? '';

      if (userName.isNotEmpty) {
        // Si no hay ejecutivos o el primero está vacío, agregar el usuario actual
        if (widget.quotationModel.salesExecutiveControllers.isEmpty) {
          widget.quotationModel.addSalesExecutive();
        }

        if (widget.quotationModel.salesExecutiveControllers.isNotEmpty &&
            widget.quotationModel.salesExecutiveControllers[0].text.isEmpty) {
          widget.quotationModel.salesExecutiveControllers[0].text = userName;
          debugPrint('✅ Ejecutivo de ventas por defecto: $userName');
        }
      }
    } catch (e) {
      debugPrint('❌ Error obteniendo usuario para ejecutivo por defecto: $e');
    }
  }

  @override
  void dispose() {
    _customerSearchController.dispose();
    super.dispose();
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year}';
  }

  /// Método para mostrar las cotizaciones existentes del cliente
  Future<void> _showCustomerQuotations(String customerId) async {
    if (customerId.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Seleccione un cliente primero')),
      );
      return;
    }

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Cotizaciones del Cliente'),
        content: SizedBox(
          width: double.maxFinite,
          child: FutureBuilder<List<QuotationRes>>(
            future: _quotationService.getQuotationsByCustomer(
              idCustomer: int.tryParse(customerId) ?? 0,
            ).then((response) => response.data ?? <QuotationRes>[]),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(child: CircularProgressIndicator());
              }
              if (snapshot.hasError) {
                return Center(child: Text('Error: ${snapshot.error}'));
              }
              final quotations = snapshot.data ?? [];
              if (quotations.isEmpty) {
                return const Center(
                  child: Text('No hay cotizaciones previas para este cliente'),
                );
              }
              return ListView.builder(
                itemCount: quotations.length,
                itemBuilder: (context, index) {
                  final quotation = quotations[index];
                  return ListTile(
                    title: Text('Folio: ${quotation.folio}'),
                    subtitle: Text('Estado: ${quotation.status}'),
                    trailing: Text(_formatDate(quotation.saleDate)),
                  );
                },
              );
            },
          ),
        ),
        actions: [
          AppButton(
            label: 'Cerrar',
            variant: 'text',
            colorType: 'secondary',
            onPressed: () => Navigator.pop(context),
          ),
        ],
      ),
    );
  }

  Widget _buildCustomerStatusWidget()  {
    final hasCustomerId =
        widget.quotationModel.customer.customerIdController.text.isNotEmpty;

    return hasCustomerId
        ? Container(
            margin: const EdgeInsets.only(bottom: 8),
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
                  color: Colors.green.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.green.withValues(alpha: 0.3)),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(
                  Icons.check_circle,
                  color: Colors.green.shade600,
                  size: 16,
                ),
                const SizedBox(width: 6),
                Text(
                  'ID: ${widget.quotationModel.customer.customerIdController.text}',
                  style: TextStyle(
                    color: Colors.green.shade700,
                    fontSize: 12,
                    fontWeight: FontWeight.w500,
                  ),
                ),
                const SizedBox(width: 8),
                GestureDetector(
                  onTap: () {
                    _showCustomerQuotations(
                      widget.quotationModel.customer.customerIdController.text,
                    );
                  },
                  child: Tooltip(
                    message: 'Ver cotizaciones anteriores',
                    child: Icon(
                      Icons.history,
                      color: Colors.blue.shade600,
                      size: 16,
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                GestureDetector(
                  onTap: () {
                    setState(() {
                      widget.quotationModel.customer.customerIdController
                          .clear();
                      widget.quotationModel.customer.fullNameController.clear();
                      widget.quotationModel.customer.codeController.clear();
                      widget.quotationModel.customer.emailController.clear();
                      widget.quotationModel.customer.phoneController.clear();
                      _customerSearchController.clear();
                    });
                  },
                  child: Tooltip(
                    message: 'Limpiar cliente seleccionado',
                    child: Icon(
                      Icons.close,
                      color: Colors.red.shade600,
                      size: 16,
                    ),
                  ),
                ),
              ],
            ),
          )
        : Container(
            margin: const EdgeInsets.only(bottom: 8),
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
                  color: Colors.blue.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.blue.withValues(alpha: 0.3)),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(Icons.person_add, color: Colors.blue.shade600, size: 16),
                const SizedBox(width: 6),
                Text(
                  'Nuevo cliente',
                  style: TextStyle(
                    color: Colors.blue.shade700,
                    fontSize: 12,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ],
            ),
          );
  }

  List<FormFieldConfig> _buildCustomerFields() {
    return [
      FormFieldConfig(
        label: 'Buscar Cliente Existente',
        type: FormFieldType.suggest,
        controller: _customerSearchController,
        hint: 'Escriba al menos 2 caracteres para buscar...',
        onSearch: (query) async {
          try {
            final response = await _customerService.searchCustomers(
              name: query,
            );
            if (response.success && response.data != null) {
              debugPrint('✅ Clientes encontrados: ${response.data!.length}');
              // Convertir explícitamente a List<dynamic> para el widget
              final List<dynamic> dynamicList = response.data!.cast<dynamic>();
              return dynamicList;
            } else {
              debugPrint('❌ Error en búsqueda: ${response.message}');
              return <dynamic>[];
            }
          } catch (e) {
            debugPrint('❌ Excepción en búsqueda: $e');
            if (context.mounted) {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text('Error al buscar clientes: ${e.toString()}'),
                  backgroundColor: Colors.red,
                ),
              );
            }
            return <dynamic>[];
          }
        },
        itemToString: (customer) {
          final customerRes = customer as CustomerRes;
          return '${customerRes.fullName} - ${customerRes.email}';
        },
        onItemSelected: (selectedCustomer) {
          final customer = selectedCustomer as CustomerRes;
          setState(() {
            // Llenar los campos del cliente
            widget.quotationModel.customer.customerIdController.text =
                customer.customerId; // Usa el getter que convierte idCustomer a String
            widget.quotationModel.customer.fullNameController.text =
                customer.fullName; // Usa el getter que combina name + lastName
            widget.quotationModel.customer.codeController.text = customer.code; // Usa el getter que convierte externalId
            widget.quotationModel.customer.emailController.text =
                customer.email;
            widget.quotationModel.customer.phoneController.text =
                customer.phoneNumber; // Usa directamente phoneNumber
            
            // Limpiar el campo de búsqueda
            _customerSearchController.clear();
          });
          
          // Mostrar mensaje de éxito
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Cliente "${customer.fullName}" seleccionado'),
              backgroundColor: Colors.green,
              duration: const Duration(seconds: 2),
            ),
          );
        },
      ),
      FormFieldConfig(
        label: 'Nombre Completo',
        controller: widget.quotationModel.customer.fullNameController,
        isRequired: true,
        hint: 'Nombre del cliente',
      ),
      FormFieldConfig(
        label: 'Código',
        controller: widget.quotationModel.customer.codeController,
        hint: 'Código del cliente',
      ),
      FormFieldConfig(
        label: 'Teléfono',
        controller: widget.quotationModel.customer.phoneController,
        hint: 'Teléfono del cliente',
      ),
      FormFieldConfig(
        label: 'Email',
        type: FormFieldType.email,
        controller: widget.quotationModel.customer.emailController,
        hint: 'Email del cliente',
      ),
    ];
  }

  List<FormFieldConfig> _buildSalesExecutiveFields() {
    final fields = <FormFieldConfig>[];

    for (
      int i = 0;
      i < widget.quotationModel.salesExecutiveControllers.length;
      i++
    ) {
      fields.add(
        FormFieldConfig(
          label: 'Ejecutivo $i',
          controller: widget.quotationModel.salesExecutiveControllers[i],
          hint: 'Nombre del ejecutivo de ventas',
          suffixIcon: GestureDetector(
            onTap: () {
              setState(() {
                widget.quotationModel.removeSalesExecutive(i);
              });
            },
            child: Icon(Icons.close, color: Colors.red.shade600),
          ),
        ),
      );
    }

    return fields;
  }

  List<FormFieldConfig> _buildFollowupFields() {
    final fields = <FormFieldConfig>[];

    for (int i = 0; i < widget.quotationModel.followups.length; i++) {
      final followup = widget.quotationModel.followups[i];

      fields.addAll([
        FormFieldConfig(
          label: 'Comentario - Seguimiento $i',
          type: FormFieldType.textarea,
          controller: followup.commentController,
          maxLines: 2,
          hint: 'Detalle del seguimiento',
        ),
        FormFieldConfig(
          label: 'Usuario - Seguimiento $i',
          controller: followup.userIdController,
          hint: 'ID del usuario que hace el seguimiento',
          enabled: false, // Solo lectura
        ),
        FormFieldConfig(
          label: 'Fecha - Seguimiento $i',
          type: FormFieldType.date,
          controller: TextEditingController(text: _formatDate(followup.date)),
          onDateChanged: (date) {
            if (date != null) {
              setState(() {
                // Actualizar la fecha del seguimiento
              });
            }
          },
        ),
      ]);
    }

    return fields;
  }

  /// Construye el contenido del formulario
  Widget _buildFormContent() {
    return AppForm(
      formKey: _formKey,
      padding: EdgeInsets.zero,
      sections: [
        // Sección Principal
        FormSection(
          title: widget.isUpdate ? 'Editar Cotización' : 'Nueva Cotización',
          fields: [
            FormFieldConfig(
              label: 'Folio',
              type: FormFieldType.number,
              controller: widget.quotationModel.folioController,
              isRequired: true,
              keyboardType: TextInputType.number,
              inputFormatters: [FilteringTextInputFormatter.digitsOnly],
              enabled: !widget.isUpdate, // Solo editable en modo crear
            ),
            FormFieldConfig(
              label: 'Fecha de Venta',
              type: FormFieldType.date,
              controller: TextEditingController(
                text: _formatDate(widget.quotationModel.saleDate),
              ),
              isRequired: true,
              onDateChanged: (date) {
                if (date != null) {
                  setState(() {
                    // Actualizar la fecha
                  });
                }
              },
            ),
            FormFieldConfig(
              label: 'Comentario General',
              type: FormFieldType.textarea,
              controller: widget.quotationModel.generalCommentController,
              maxLines: 3,
            ),
          ],
        ),

        // Sección de Cliente
        FormSection(
          title: 'Información del Cliente',
          isCollapsible: true,
          headerWidget: _buildCustomerStatusWidget(),
          fields: _buildCustomerFields(),
        ),

        // Sección de Ejecutivos de Ventas
        FormSection(
          title: 'Ejecutivos de Ventas',
          isCollapsible: true,
          initiallyExpanded: false,
          headerWidget: Padding(
            padding: const EdgeInsets.symmetric(vertical: 8.0),
            child: Row(
              children: [
                const Expanded(
                  child: Text(
                    'Agregue los ejecutivos de ventas asignados',
                    style: TextStyle(fontSize: 14, color: Colors.grey),
                  ),
                ),
                AppButton(
                  label: 'Agregar Ejecutivo',
                  size: 'small',
                  colorType: 'primary',
                  onPressed: () {
                    setState(() {
                      widget.quotationModel.addSalesExecutive();
                    });
                  },
                ),
              ],
            ),
          ),
          fields: _buildSalesExecutiveFields(),
        ),

        // Sección de Seguimientos
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
                    'Historial de seguimientos a la cotización',
                    style: TextStyle(fontSize: 14, color: Colors.grey),
                  ),
                ),
                AppButton(
                  label: 'Agregar Seguimiento',
                  size: 'small',
                  colorType: 'primary',
                  onPressed: () async {
                    try {
                      final userId = await HiveService.getCache('user.id');
                      setState(() {
                        widget.quotationModel.addFollowup();
                        if (widget.quotationModel.followups.isNotEmpty &&
                            userId != null && userId.isNotEmpty) {
                          widget
                                  .quotationModel
                                  .followups
                                  .last
                                  .userIdController
                                  .text =
                              userId;
                          debugPrint('✅ UserId asignado automáticamente a nuevo followup: $userId');
                        }
                      });
                    } catch (e) {
                      debugPrint('❌ Error obteniendo userId para followup: $e');
                      // Agregar followup sin userId si hay error
                      setState(() {
                        widget.quotationModel.addFollowup();
                      });
                    }
                  },
                ),
              ],
            ),
          ),
          fields: _buildFollowupFields(),
        ),
      ],
    );
  }

  /// Valida el formulario
  bool validate() {
    return _formKey.currentState?.validate() ?? false;
  }

  /// Guarda los datos del formulario
  void save() {
    if (_formKey.currentState != null) {
      _formKey.currentState!.save();
    }
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: _buildFormContent(),
    );
  }
}

// =============================================================================
// CLASES SEPARADAS PARA CREAR Y EDITAR COTIZACIONES
// =============================================================================

/// Widget para crear nuevas cotizaciones
class QuotationCreateWidget extends StatefulWidget {
  final QuotationReq quotationModel;

  const QuotationCreateWidget({
    super.key,
    required this.quotationModel,
  });

  @override
  State<QuotationCreateWidget> createState() => _QuotationCreateWidgetState();
}

class _QuotationCreateWidgetState extends State<QuotationCreateWidget> {
  late final CustomerService _customerService;
  final QuotationManagerService _quotationService = const QuotationManagerService();
  late GlobalKey<FormState> _formKey;
  late TextEditingController _customerSearchController;

  @override
  void initState() {
    super.initState();
    _customerService = CustomerService();
    _formKey = GlobalKey<FormState>();
    _customerSearchController = TextEditingController();

    // Agregar el usuario actual como ejecutivo de ventas por defecto
    _initializeDefaultSalesExecutive();
  }

  /// Inicializa el ejecutivo de ventas por defecto con el usuario actual
  Future<void> _initializeDefaultSalesExecutive() async {
    try {
      final userNameCache = await HiveService.getCache('user.name');
      final userName = userNameCache ?? '';

      if (userName.isNotEmpty) {
        // Si no hay ejecutivos o el primero está vacío, agregar el usuario actual
        if (widget.quotationModel.salesExecutiveControllers.isEmpty) {
          widget.quotationModel.addSalesExecutive();
        }

        if (widget.quotationModel.salesExecutiveControllers.isNotEmpty &&
            widget.quotationModel.salesExecutiveControllers[0].text.isEmpty) {
          widget.quotationModel.salesExecutiveControllers[0].text = userName;
          debugPrint('✅ Ejecutivo de ventas por defecto: $userName');
        }
      }
    } catch (e) {
      debugPrint('❌ Error obteniendo usuario para ejecutivo por defecto: $e');
    }
  }

  @override
  void dispose() {
    _customerSearchController.dispose();
    super.dispose();
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year}';
  }

  /// Método para mostrar las cotizaciones existentes del cliente
  Future<void> _showCustomerQuotations(String customerId) async {
    if (customerId.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Seleccione un cliente primero')),
      );
      return;
    }

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Cotizaciones del Cliente'),
        content: SizedBox(
          width: double.maxFinite,
          child: FutureBuilder<List<QuotationRes>>(
            future: _quotationService.getQuotationsByCustomer(
              idCustomer: int.tryParse(customerId) ?? 0,
            ).then((response) => response.data ?? <QuotationRes>[]),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(child: CircularProgressIndicator());
              }
              if (snapshot.hasError) {
                return Center(child: Text('Error: ${snapshot.error}'));
              }
              final quotations = snapshot.data ?? [];
              if (quotations.isEmpty) {
                return const Center(
                  child: Text('No hay cotizaciones previas para este cliente'),
                );
              }
              return ListView.builder(
                itemCount: quotations.length,
                itemBuilder: (context, index) {
                  final quotation = quotations[index];
                  return ListTile(
                    title: Text('Folio: ${quotation.folio}'),
                    subtitle: Text('Estado: ${quotation.status}'),
                    trailing: Text(_formatDate(quotation.saleDate)),
                  );
                },
              );
            },
          ),
        ),
        actions: [
          AppButton(
            label: 'Cerrar',
            variant: 'text',
            colorType: 'secondary',
            onPressed: () => Navigator.pop(context),
          ),
        ],
      ),
    );
  }

  Widget _buildCustomerStatusWidget()  {
    final hasCustomerId =
        widget.quotationModel.customer.customerIdController.text.isNotEmpty;

    return hasCustomerId
        ? Container(
            margin: const EdgeInsets.only(bottom: 8),
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
                  color: Colors.green.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.green.withValues(alpha: 0.3)),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(
                  Icons.check_circle,
                  color: Colors.green.shade600,
                  size: 16,
                ),
                const SizedBox(width: 6),
                Text(
                  'ID: ${widget.quotationModel.customer.customerIdController.text}',
                  style: TextStyle(
                    color: Colors.green.shade700,
                    fontSize: 12,
                    fontWeight: FontWeight.w500,
                  ),
                ),
                const SizedBox(width: 8),
                GestureDetector(
                  onTap: () {
                    _showCustomerQuotations(
                      widget.quotationModel.customer.customerIdController.text,
                    );
                  },
                  child: Tooltip(
                    message: 'Ver cotizaciones anteriores',
                    child: Icon(
                      Icons.history,
                      color: Colors.blue.shade600,
                      size: 16,
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                GestureDetector(
                  onTap: () {
                    setState(() {
                      widget.quotationModel.customer.customerIdController
                          .clear();
                      widget.quotationModel.customer.fullNameController.clear();
                      widget.quotationModel.customer.codeController.clear();
                      widget.quotationModel.customer.emailController.clear();
                      widget.quotationModel.customer.phoneController.clear();
                      _customerSearchController.clear();
                    });
                  },
                  child: Tooltip(
                    message: 'Limpiar cliente seleccionado',
                    child: Icon(
                      Icons.close,
                      color: Colors.red.shade600,
                      size: 16,
                    ),
                  ),
                ),
              ],
            ),
          )
        : Container(
            margin: const EdgeInsets.only(bottom: 8),
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
                  color: Colors.blue.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.blue.withValues(alpha: 0.3)),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(Icons.person_add, color: Colors.blue.shade600, size: 16),
                const SizedBox(width: 6),
                Text(
                  'Nuevo cliente',
                  style: TextStyle(
                    color: Colors.blue.shade700,
                    fontSize: 12,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ],
            ),
          );
  }

  List<FormFieldConfig> _buildCustomerFields() {
    return [
      FormFieldConfig(
        label: 'Buscar Cliente Existente',
        type: FormFieldType.suggest,
        controller: _customerSearchController,
        hint: 'Escriba al menos 2 caracteres para buscar...',
        onSearch: (query) async {
          try {
            final response = await _customerService.searchCustomers(
              name: query,
            );
            if (response.success && response.data != null) {
              debugPrint('✅ Clientes encontrados: ${response.data!.length}');
              // Convertir explícitamente a List<dynamic> para el widget
              final List<dynamic> dynamicList = response.data!.cast<dynamic>();
              return dynamicList;
            } else {
              debugPrint('❌ Error en búsqueda: ${response.message}');
              return <dynamic>[];
            }
          } catch (e) {
            debugPrint('❌ Excepción en búsqueda: $e');
            if (context.mounted) {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text('Error al buscar clientes: ${e.toString()}'),
                  backgroundColor: Colors.red,
                ),
              );
            }
            return <dynamic>[];
          }
        },
        itemToString: (customer) {
          final customerRes = customer as CustomerRes;
          return '${customerRes.fullName} - ${customerRes.email}';
        },
        onItemSelected: (selectedCustomer) {
          final customer = selectedCustomer as CustomerRes;
          setState(() {
            // Llenar los campos del cliente
            widget.quotationModel.customer.customerIdController.text =
                customer.customerId; // Usa el getter que convierte idCustomer a String
            widget.quotationModel.customer.fullNameController.text =
                customer.fullName; // Usa el getter que combina name + lastName
            widget.quotationModel.customer.codeController.text = customer.code; // Usa el getter que convierte externalId
            widget.quotationModel.customer.emailController.text =
                customer.email;
            widget.quotationModel.customer.phoneController.text =
                customer.phoneNumber; // Usa directamente phoneNumber

            // Limpiar el campo de búsqueda
            _customerSearchController.clear();
          });

          // Mostrar mensaje de éxito
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Cliente "${customer.fullName}" seleccionado'),
              backgroundColor: Colors.green,
              duration: const Duration(seconds: 2),
            ),
          );
        },
      ),
      FormFieldConfig(
        label: 'Nombre Completo',
        controller: widget.quotationModel.customer.fullNameController,
        isRequired: true,
        hint: 'Nombre del cliente',
      ),
      FormFieldConfig(
        label: 'Código',
        controller: widget.quotationModel.customer.codeController,
        hint: 'Código del cliente',
      ),
      FormFieldConfig(
        label: 'Teléfono',
        controller: widget.quotationModel.customer.phoneController,
        hint: 'Teléfono del cliente',
      ),
      FormFieldConfig(
        label: 'Email',
        type: FormFieldType.email,
        controller: widget.quotationModel.customer.emailController,
        hint: 'Email del cliente',
      ),
    ];
  }

  List<FormFieldConfig> _buildSalesExecutiveFields() {
    final fields = <FormFieldConfig>[];

    for (
      int i = 0;
      i < widget.quotationModel.salesExecutiveControllers.length;
      i++
    ) {
      fields.add(
        FormFieldConfig(
          label: 'Ejecutivo $i',
          controller: widget.quotationModel.salesExecutiveControllers[i],
          hint: 'Nombre del ejecutivo de ventas',
          suffixIcon: GestureDetector(
            onTap: () {
              setState(() {
                widget.quotationModel.removeSalesExecutive(i);
              });
            },
            child: Icon(Icons.close, color: Colors.red.shade600),
          ),
        ),
      );
    }

    return fields;
  }

  List<FormFieldConfig> _buildFollowupFields() {
    final fields = <FormFieldConfig>[];

    for (int i = 0; i < widget.quotationModel.followups.length; i++) {
      final followup = widget.quotationModel.followups[i];

      fields.addAll([
        FormFieldConfig(
          label: 'Comentario - Seguimiento $i',
          type: FormFieldType.textarea,
          controller: followup.commentController,
          maxLines: 2,
          hint: 'Detalle del seguimiento',
        ),
        FormFieldConfig(
          label: 'Usuario - Seguimiento $i',
          controller: followup.userIdController,
          hint: 'ID del usuario que hace el seguimiento',
          enabled: false, // Solo lectura
        ),
        FormFieldConfig(
          label: 'Fecha - Seguimiento $i',
          type: FormFieldType.date,
          controller: TextEditingController(text: _formatDate(followup.date)),
          onDateChanged: (date) {
            if (date != null) {
              setState(() {
                // Actualizar la fecha del seguimiento
              });
            }
          },
        ),
      ]);
    }

    return fields;
  }

  /// Construye el contenido del formulario para crear cotización
  Widget _buildFormContent() {
    return AppForm(
      formKey: _formKey,
      padding: EdgeInsets.zero,
      sections: [
        // Sección Principal
        FormSection(
          title: 'Nueva Cotización',
          fields: [
            FormFieldConfig(
              label: 'Folio',
              type: FormFieldType.number,
              controller: widget.quotationModel.folioController,
              isRequired: true,
              keyboardType: TextInputType.number,
              inputFormatters: [FilteringTextInputFormatter.digitsOnly],
            ),
            FormFieldConfig(
              label: 'Fecha de Venta',
              type: FormFieldType.date,
              controller: TextEditingController(
                text: _formatDate(widget.quotationModel.saleDate),
              ),
              isRequired: true,
              onDateChanged: (date) {
                if (date != null) {
                  setState(() {
                    // Actualizar la fecha
                  });
                }
              },
            ),
            FormFieldConfig(
              label: 'Comentario General',
              type: FormFieldType.textarea,
              controller: widget.quotationModel.generalCommentController,
              maxLines: 3,
            ),
          ],
        ),

        // Sección de Cliente
        FormSection(
          title: 'Información del Cliente',
          isCollapsible: true,
          headerWidget: _buildCustomerStatusWidget(),
          fields: _buildCustomerFields(),
        ),

        // Sección de Ejecutivos de Ventas
        FormSection(
          title: 'Ejecutivos de Ventas',
          isCollapsible: true,
          initiallyExpanded: false,
          headerWidget: Padding(
            padding: const EdgeInsets.symmetric(vertical: 8.0),
            child: Row(
              children: [
                const Expanded(
                  child: Text(
                    'Agregue los ejecutivos de ventas asignados',
                    style: TextStyle(fontSize: 14, color: Colors.grey),
                  ),
                ),
                AppButton(
                  label: 'Agregar Ejecutivo',
                  size: 'small',
                  colorType: 'primary',
                  onPressed: () {
                    setState(() {
                      widget.quotationModel.addSalesExecutive();
                    });
                  },
                ),
              ],
            ),
          ),
          fields: _buildSalesExecutiveFields(),
        ),

        // Sección de Seguimientos
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
                    'Historial de seguimientos a la cotización',
                    style: TextStyle(fontSize: 14, color: Colors.grey),
                  ),
                ),
                AppButton(
                  label: 'Agregar Seguimiento',
                  size: 'small',
                  colorType: 'primary',
                  onPressed: () async {
                    try {
                      final userId = await HiveService.getCache('user.id');
                      setState(() {
                        widget.quotationModel.addFollowup();
                        if (widget.quotationModel.followups.isNotEmpty &&
                            userId != null && userId.isNotEmpty) {
                          widget
                                  .quotationModel
                                  .followups
                                  .last
                                  .userIdController
                                  .text =
                              userId;
                          debugPrint('✅ UserId asignado automáticamente a nuevo followup: $userId');
                        }
                      });
                    } catch (e) {
                      debugPrint('❌ Error obteniendo userId para followup: $e');
                      // Agregar followup sin userId si hay error
                      setState(() {
                        widget.quotationModel.addFollowup();
                      });
                    }
                  },
                ),
              ],
            ),
          ),
          fields: _buildFollowupFields(),
        ),
      ],
    );
  }



  /// Valida el formulario
  bool validate() {
    return _formKey.currentState?.validate() ?? false;
  }

  /// Guarda los datos del formulario
  void save() {
    if (_formKey.currentState != null) {
      _formKey.currentState!.save();
    }
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: _buildFormContent(),
    );
  }
}

/// Widget para editar cotizaciones existentes
class QuotationEditWidget extends StatefulWidget {
  final QuotationReq quotationModel;
  final QuotationRes existingQuotation;

  const QuotationEditWidget({
    super.key,
    required this.quotationModel,
    required this.existingQuotation,
  });

  @override
  State<QuotationEditWidget> createState() => _QuotationEditWidgetState();
}

class _QuotationEditWidgetState extends State<QuotationEditWidget> {
  late final CustomerService _customerService;
  final QuotationManagerService _quotationService = const QuotationManagerService();
  late GlobalKey<FormState> _formKey;
  late TextEditingController _customerSearchController;

  @override
  void initState() {
    super.initState();
    _customerService = CustomerService();
    _formKey = GlobalKey<FormState>();
    _customerSearchController = TextEditingController();

    // Cargar datos de la cotización existente
    _loadExistingQuotationData();
  }

  /// Carga los datos de la cotización existente en el formulario
  void _loadExistingQuotationData() {
    final quotation = widget.existingQuotation;

    // Cargar datos básicos
    widget.quotationModel.folioController.text = quotation.folio.toString();
    widget.quotationModel.generalCommentController.text = quotation.generalComment;

    // Cargar datos del cliente
    widget.quotationModel.customer.customerIdController.text = quotation.customerId;
    widget.quotationModel.customer.fullNameController.text = quotation.customer.fullName;
    widget.quotationModel.customer.codeController.text = quotation.customer.externalId.toString();
    widget.quotationModel.customer.emailController.text = quotation.customer.email;
    widget.quotationModel.customer.phoneController.text = quotation.customer.phoneNumber;

    // Cargar ejecutivos de ventas
    widget.quotationModel.salesExecutiveControllers.clear();
    for (final executive in quotation.salesExecutives) {
      widget.quotationModel.addSalesExecutive();
      widget.quotationModel.salesExecutiveControllers.last.text = executive;
    }

    // Cargar seguimientos
    widget.quotationModel.followups.clear();
    for (final followup in quotation.followups) {
      widget.quotationModel.addFollowup();
      final lastFollowup = widget.quotationModel.followups.last;
      lastFollowup.commentController.text = followup.comment;
      lastFollowup.userIdController.text = followup.userId;
      // La fecha se maneja automáticamente por el modelo
    }

    debugPrint('✅ Datos de cotización cargados para edición');
  }

  @override
  void dispose() {
    _customerSearchController.dispose();
    super.dispose();
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year}';
  }

  /// Método para mostrar las cotizaciones existentes del cliente
  Future<void> _showCustomerQuotations(String customerId) async {
    if (customerId.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Seleccione un cliente primero')),
      );
      return;
    }

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Cotizaciones del Cliente'),
        content: SizedBox(
          width: double.maxFinite,
          child: FutureBuilder<List<QuotationRes>>(
            future: _quotationService.getQuotationsByCustomer(
              idCustomer: int.tryParse(customerId) ?? 0,
            ).then((response) => response.data ?? <QuotationRes>[]),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(child: CircularProgressIndicator());
              }
              if (snapshot.hasError) {
                return Center(child: Text('Error: ${snapshot.error}'));
              }
              final quotations = snapshot.data ?? [];
              if (quotations.isEmpty) {
                return const Center(
                  child: Text('No hay cotizaciones previas para este cliente'),
                );
              }
              return ListView.builder(
                itemCount: quotations.length,
                itemBuilder: (context, index) {
                  final quotation = quotations[index];
                  return ListTile(
                    title: Text('Folio: ${quotation.folio}'),
                    subtitle: Text('Estado: ${quotation.status}'),
                    trailing: Text(_formatDate(quotation.saleDate)),
                  );
                },
              );
            },
          ),
        ),
        actions: [
          AppButton(
            label: 'Cerrar',
            variant: 'text',
            colorType: 'secondary',
            onPressed: () => Navigator.pop(context),
          ),
        ],
      ),
    );
  }

  Widget _buildCustomerStatusWidget()  {
    final hasCustomerId =
        widget.quotationModel.customer.customerIdController.text.isNotEmpty;

    return hasCustomerId
        ? Container(
            margin: const EdgeInsets.only(bottom: 8),
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
                  color: Colors.green.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.green.withValues(alpha: 0.3)),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(
                  Icons.check_circle,
                  color: Colors.green.shade600,
                  size: 16,
                ),
                const SizedBox(width: 6),
                Text(
                  'ID: ${widget.quotationModel.customer.customerIdController.text}',
                  style: TextStyle(
                    color: Colors.green.shade700,
                    fontSize: 12,
                    fontWeight: FontWeight.w500,
                  ),
                ),
                const SizedBox(width: 8),
                GestureDetector(
                  onTap: () {
                    _showCustomerQuotations(
                      widget.quotationModel.customer.customerIdController.text,
                    );
                  },
                  child: Tooltip(
                    message: 'Ver cotizaciones anteriores',
                    child: Icon(
                      Icons.history,
                      color: Colors.blue.shade600,
                      size: 16,
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                GestureDetector(
                  onTap: () {
                    setState(() {
                      widget.quotationModel.customer.customerIdController
                          .clear();
                      widget.quotationModel.customer.fullNameController.clear();
                      widget.quotationModel.customer.codeController.clear();
                      widget.quotationModel.customer.emailController.clear();
                      widget.quotationModel.customer.phoneController.clear();
                      _customerSearchController.clear();
                    });
                  },
                  child: Tooltip(
                    message: 'Limpiar cliente seleccionado',
                    child: Icon(
                      Icons.close,
                      color: Colors.red.shade600,
                      size: 16,
                    ),
                  ),
                ),
              ],
            ),
          )
        : Container(
            margin: const EdgeInsets.only(bottom: 8),
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
                  color: Colors.blue.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.blue.withValues(alpha: 0.3)),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(Icons.person_add, color: Colors.blue.shade600, size: 16),
                const SizedBox(width: 6),
                Text(
                  'Nuevo cliente',
                  style: TextStyle(
                    color: Colors.blue.shade700,
                    fontSize: 12,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ],
            ),
          );
  }

  List<FormFieldConfig> _buildCustomerFields() {
    return [
      FormFieldConfig(
        label: 'Buscar Cliente Existente',
        type: FormFieldType.suggest,
        controller: _customerSearchController,
        hint: 'Escriba al menos 2 caracteres para buscar...',
        onSearch: (query) async {
          try {
            final response = await _customerService.searchCustomers(
              name: query,
            );
            if (response.success && response.data != null) {
              debugPrint('✅ Clientes encontrados: ${response.data!.length}');
              // Convertir explícitamente a List<dynamic> para el widget
              final List<dynamic> dynamicList = response.data!.cast<dynamic>();
              return dynamicList;
            } else {
              debugPrint('❌ Error en búsqueda: ${response.message}');
              return <dynamic>[];
            }
          } catch (e) {
            debugPrint('❌ Excepción en búsqueda: $e');
            if (context.mounted) {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text('Error al buscar clientes: ${e.toString()}'),
                  backgroundColor: Colors.red,
                ),
              );
            }
            return <dynamic>[];
          }
        },
        itemToString: (customer) {
          final customerRes = customer as CustomerRes;
          return '${customerRes.fullName} - ${customerRes.email}';
        },
        onItemSelected: (selectedCustomer) {
          final customer = selectedCustomer as CustomerRes;
          setState(() {
            // Llenar los campos del cliente
            widget.quotationModel.customer.customerIdController.text =
                customer.customerId; // Usa el getter que convierte idCustomer a String
            widget.quotationModel.customer.fullNameController.text =
                customer.fullName; // Usa el getter que combina name + lastName
            widget.quotationModel.customer.codeController.text = customer.code; // Usa el getter que convierte externalId
            widget.quotationModel.customer.emailController.text =
                customer.email;
            widget.quotationModel.customer.phoneController.text =
                customer.phoneNumber; // Usa directamente phoneNumber

            // Limpiar el campo de búsqueda
            _customerSearchController.clear();
          });

          // Mostrar mensaje de éxito
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Cliente "${customer.fullName}" seleccionado'),
              backgroundColor: Colors.green,
              duration: const Duration(seconds: 2),
            ),
          );
        },
      ),
      FormFieldConfig(
        label: 'Nombre Completo',
        controller: widget.quotationModel.customer.fullNameController,
        isRequired: true,
        hint: 'Nombre del cliente',
      ),
      FormFieldConfig(
        label: 'Código',
        controller: widget.quotationModel.customer.codeController,
        hint: 'Código del cliente',
      ),
      FormFieldConfig(
        label: 'Teléfono',
        controller: widget.quotationModel.customer.phoneController,
        hint: 'Teléfono del cliente',
      ),
      FormFieldConfig(
        label: 'Email',
        type: FormFieldType.email,
        controller: widget.quotationModel.customer.emailController,
        hint: 'Email del cliente',
      ),
    ];
  }

  List<FormFieldConfig> _buildSalesExecutiveFields() {
    final fields = <FormFieldConfig>[];

    for (
      int i = 0;
      i < widget.quotationModel.salesExecutiveControllers.length;
      i++
    ) {
      fields.add(
        FormFieldConfig(
          label: 'Ejecutivo $i',
          controller: widget.quotationModel.salesExecutiveControllers[i],
          hint: 'Nombre del ejecutivo de ventas',
          suffixIcon: GestureDetector(
            onTap: () {
              setState(() {
                widget.quotationModel.removeSalesExecutive(i);
              });
            },
            child: Icon(Icons.close, color: Colors.red.shade600),
          ),
        ),
      );
    }

    return fields;
  }

  List<FormFieldConfig> _buildFollowupFields() {
    final fields = <FormFieldConfig>[];

    for (int i = 0; i < widget.quotationModel.followups.length; i++) {
      final followup = widget.quotationModel.followups[i];

      fields.addAll([
        FormFieldConfig(
          label: 'Comentario - Seguimiento $i',
          type: FormFieldType.textarea,
          controller: followup.commentController,
          maxLines: 2,
          hint: 'Detalle del seguimiento',
        ),
        FormFieldConfig(
          label: 'Usuario - Seguimiento $i',
          controller: followup.userIdController,
          hint: 'ID del usuario que hace el seguimiento',
          enabled: false, // Solo lectura
        ),
        FormFieldConfig(
          label: 'Fecha - Seguimiento $i',
          type: FormFieldType.date,
          controller: TextEditingController(text: _formatDate(followup.date)),
          onDateChanged: (date) {
            if (date != null) {
              setState(() {
                // Actualizar la fecha del seguimiento
              });
            }
          },
        ),
      ]);
    }

    return fields;
  }

  /// Construye el contenido del formulario para editar cotización
  Widget _buildFormContent() {
    return AppForm(
      formKey: _formKey,
      padding: EdgeInsets.zero,
      sections: [
        // Sección Principal
        FormSection(
          title: 'Editar Cotización - Folio ${widget.existingQuotation.folio}',
          fields: [
            FormFieldConfig(
              label: 'Folio',
              type: FormFieldType.number,
              controller: widget.quotationModel.folioController,
              isRequired: true,
              keyboardType: TextInputType.number,
              inputFormatters: [FilteringTextInputFormatter.digitsOnly],
              enabled: false, // El folio no se puede editar
            ),
            FormFieldConfig(
              label: 'Fecha de Venta',
              type: FormFieldType.date,
              controller: TextEditingController(
                text: _formatDate(widget.quotationModel.saleDate),
              ),
              isRequired: true,
              onDateChanged: (date) {
                if (date != null) {
                  setState(() {
                    // Actualizar la fecha
                  });
                }
              },
            ),
            FormFieldConfig(
              label: 'Comentario General',
              type: FormFieldType.textarea,
              controller: widget.quotationModel.generalCommentController,
              maxLines: 3,
            ),
          ],
        ),

        // Sección de Cliente
        FormSection(
          title: 'Información del Cliente',
          isCollapsible: true,
          headerWidget: _buildCustomerStatusWidget(),
          fields: _buildCustomerFields(),
        ),

        // Sección de Ejecutivos de Ventas
        FormSection(
          title: 'Ejecutivos de Ventas',
          isCollapsible: true,
          initiallyExpanded: false,
          headerWidget: Padding(
            padding: const EdgeInsets.symmetric(vertical: 8.0),
            child: Row(
              children: [
                const Expanded(
                  child: Text(
                    'Agregue los ejecutivos de ventas asignados',
                    style: TextStyle(fontSize: 14, color: Colors.grey),
                  ),
                ),
                AppButton(
                  label: 'Agregar Ejecutivo',
                  size: 'small',
                  colorType: 'primary',
                  onPressed: () {
                    setState(() {
                      widget.quotationModel.addSalesExecutive();
                    });
                  },
                ),
              ],
            ),
          ),
          fields: _buildSalesExecutiveFields(),
        ),

        // Sección de Seguimientos
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
                    'Historial de seguimientos a la cotización',
                    style: TextStyle(fontSize: 14, color: Colors.grey),
                  ),
                ),
                AppButton(
                  label: 'Agregar Seguimiento',
                  size: 'small',
                  colorType: 'primary',
                  onPressed: () async {
                    try {
                      final userId = await HiveService.getCache('user.id');
                      setState(() {
                        widget.quotationModel.addFollowup();
                        if (widget.quotationModel.followups.isNotEmpty &&
                            userId != null && userId.isNotEmpty) {
                          widget
                                  .quotationModel
                                  .followups
                                  .last
                                  .userIdController
                                  .text =
                              userId;
                          debugPrint('✅ UserId asignado automáticamente a nuevo followup: $userId');
                        }
                      });
                    } catch (e) {
                      debugPrint('❌ Error obteniendo userId para followup: $e');
                      // Agregar followup sin userId si hay error
                      setState(() {
                        widget.quotationModel.addFollowup();
                      });
                    }
                  },
                ),
              ],
            ),
          ),
          fields: _buildFollowupFields(),
        ),
      ],
    );
  }



  /// Valida el formulario
  bool validate() {
    return _formKey.currentState?.validate() ?? false;
  }

  /// Guarda los datos del formulario
  void save() {
    if (_formKey.currentState != null) {
      _formKey.currentState!.save();
    }
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header con título
          Row(
            children: [
              Icon(Icons.edit_outlined, color: AppColors.primaryColor),
              const SizedBox(width: 8),
              Text(
                'Editar Cotización - Folio ${widget.existingQuotation.folio}',
                style: const TextStyle(
                  fontSize: 20,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
          const SizedBox(height: 24),

          // Formulario
          _buildFormContent(),
        ],
      ),
    );
  }
}
