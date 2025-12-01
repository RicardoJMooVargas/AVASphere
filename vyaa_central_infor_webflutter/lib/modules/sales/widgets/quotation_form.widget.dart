import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../models/requests/quotation_req.module.dart';
import '../models/response/quotation_res.module.dart';
import '../../../Core/widgets/app_form.widget.dart';
import '../../../Core/widgets/app_button.widget.dart';
import '../../../Core/models/responses/customer_res.module.dart';
import '../../../Core/services/api/customer.service.dart';
import '../../../Core/services/data/cache.service.dart';
import '../services/api/quotation_manager.service.dart';

class QuotationFormWidget extends StatefulWidget {
  final QuotationReq quotationModel;
  final QuotationRes? existingQuotation;
  final VoidCallback onSave;
  final VoidCallback onCancel;
  final bool isLoading;
  final bool isUpdate;

  const QuotationFormWidget({
    super.key,
    required this.quotationModel,
    this.existingQuotation,
    required this.onSave,
    required this.onCancel,
    this.isLoading = false,
    this.isUpdate = false,
  });

  @override
  State<QuotationFormWidget> createState() => _QuotationFormWidgetState();
}

class _QuotationFormWidgetState extends State<QuotationFormWidget> {
  late final CustomerService _customerService;
  late final QuotationManagerService _quotationService;
  late GlobalKey<FormState> _formKey;

  @override
  void initState() {
    super.initState();
    _customerService = CustomerService();
    _quotationService = QuotationManagerService();
    _formKey = GlobalKey<FormState>();
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
              customerId: int.tryParse(customerId) ?? 0,
            ),
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
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cerrar'),
          ),
        ],
      ),
    );
  }

  Widget _buildCustomerStatusWidget() {
    final hasCustomerId =
        widget.quotationModel.customer.customerIdController.text.isNotEmpty;

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
                    });
                  },
                  child: Icon(
                    Icons.close,
                    color: Colors.red.shade600,
                    size: 16,
                  ),
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
        controller: TextEditingController(),
        hint: 'Escriba el nombre del cliente para buscar...',
        onSearch: (query) async {
          if (query.length < 2) {
            return <CustomerRes>[];
          }
          try {
            final response = await _customerService.searchCustomers(
              name: query,
            );
            if (response.success && response.data != null) {
              debugPrint('✅ Clientes encontrados: ${response.data!.length}');
              return response.data!;
            } else {
              debugPrint('❌ Error en búsqueda: ${response.message}');

              return <CustomerRes>[];
            }
          } catch (e) {
            debugPrint('❌ Excepción en búsqueda: $e');
            if (context.mounted) {
              ScaffoldMessenger.of(
                context,
              ).showSnackBar(SnackBar(content: Text('Error: $e')));
            }
            return <CustomerRes>[];
          }
        },
        itemToString: (customer) {
          final customerRes = customer as CustomerRes;
          return '${customerRes.fullName} - ${customerRes.code}';
        },
        onItemSelected: (selectedCustomer) {
          final customer = selectedCustomer as CustomerRes;
          setState(() {
            widget.quotationModel.customer.customerIdController.text =
                customer.customerId;
            widget.quotationModel.customer.fullNameController.text =
                customer.fullName;
            widget.quotationModel.customer.codeController.text = customer.code;
            widget.quotationModel.customer.emailController.text =
                customer.email;
            widget.quotationModel.customer.phoneController.text =
                customer.phones.isNotEmpty ? customer.phones.first : '';
          });
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

  @override
  Widget build(BuildContext context) {
    return Card(
      color: Colors.white,
      elevation: 2,
      margin: const EdgeInsets.all(16),
      child: AppForm(
        formKey: _formKey,
        padding: const EdgeInsets.all(20),
        sections: [
          // Sección Principal
          FormSection(
            title: widget.isUpdate
                ? 'Actualizar Cotización'
                : 'Nueva Cotización',
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
                      final userId = await CacheService.getUserId();
                      setState(() {
                        widget.quotationModel.addFollowup();
                        if (widget.quotationModel.followups.isNotEmpty &&
                            userId != null) {
                          widget
                                  .quotationModel
                                  .followups
                                  .last
                                  .userIdController
                                  .text =
                              userId;
                        }
                      });
                    },
                  ),
                ],
              ),
            ),
            fields: _buildFollowupFields(),
          ),
        ],
        footer: Column(
          children: [
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: AppButton(
                    label: widget.isLoading
                        ? (widget.isUpdate ? 'Actualizando...' : 'Creando...')
                        : (widget.isUpdate ? 'Actualizar' : 'Crear'),
                    colorType: widget.isUpdate ? 'warning' : 'success',
                    onPressed: widget.isLoading ? null : widget.onSave,
                  ),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: AppButton(
                    label: 'Cancelar',
                    colorType: 'secondary',
                    onPressed: widget.isLoading ? null : widget.onCancel,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
