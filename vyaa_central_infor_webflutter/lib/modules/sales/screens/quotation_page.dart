import 'package:flutter/material.dart';
import 'package:get/get.dart';
import 'package:vyaa_central_infor_webflutter/core/controllers/notification_services.dart';
import 'package:vyaa_central_infor_webflutter/core/layouts/home_sale.layout.dart';
import 'package:vyaa_central_infor_webflutter/modules/sales/models/requests/quotation_req.module.dart';
import 'package:vyaa_central_infor_webflutter/modules/sales/models/requests/quotation_update_req.module.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/system/app_navbar.widget.dart';
import '../../../core/widgets/system/app_dialog.widget.dart';
import '../../../core/widgets/buttons/app_button.widget.dart';
import '../controllers/main_sales_page.getx.dart';
import '../models/response/quotation_res.module.dart';
import '../models/response/followups_res.module.dart';
import '../widgets/quotation_form.widget.dart';
import '../widgets/quotation_list.widget.dart';
import '../widgets/quotation_detail.widget.dart';

// path: lib/modules/sales/screens/main_sales_page.dart
// route: /sales
class MainSalesPage extends StatelessWidget {
  const MainSalesPage({super.key});

  @override
  Widget build(BuildContext context) {
    // Usar Get.find si ya existe, o Get.put si no existe
    final SalesHomeController controller =
        Get.isRegistered<SalesHomeController>()
        ? Get.find<SalesHomeController>()
        : Get.put(SalesHomeController());

    return HomeSaleLayout(
      appBar: AppNavbarWidget(
        title: 'Módulo de Ventas',
        backgroundColor: AppColors.primaryColor,
      ),
      backgroundColor: AppColors.backgroundColor,
      borderWidth: 0,
      // Header derecho - Botón para crear cotización
      rightHeader: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.end,
          children: [
            AppDialogWidget(
              buttonConfig: const DialogButtonConfig(
                type: DialogButtonType.simple,
                icon: Icons.add,
                label: 'Nueva Cotización',
                color: AppColors.primaryColor,
              ),
              title: 'Crear Cotización',
              body: Obx(
                () => QuotationCreateWidget(
                  quotationModel: controller.currentQuotation.value,
                ),
              ),
              footer: Obx(
                () => Padding(
                  padding: const EdgeInsets.all(16),
                  child: Row(
                    children: [
                      Expanded(
                        child: AppButton(
                          label: controller.isCreating.value ? 'Creando...' : 'Crear Cotización',
                          colorType: 'success',
                          onPressed: controller.isCreating.value ? null : () => controller.createQuotation(),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
      // Body derecho - Lista de cotizaciones
      rightBody: Obx(
        () => QuotationListWidget(
          quotations: controller.quotations,
          isLoading: controller.isLoading.value,
          onRefresh: () => controller.refreshData(),
          onEdit: (quotation) => _showEditDialog(context, controller, quotation),
          onDelete: (quotation) => _showDeleteConfirmation(context, controller, quotation),
          onStatusChange: (quotation, newStatus) => _showStatusChangeDialog(context, controller, quotation, newStatus),
          onView: (quotation) => AppDialogWidget.show(
            context: context,
            title: 'Detalles de Cotización',
            body: QuotationDetailWidget(
              quotation: quotation,
              onEdit: () {
                Navigator.of(context).pop();
                _showEditDialog(context, controller, quotation);
              },
              onClose: () => Navigator.of(context).pop(),
              onAddFollowup: () {
              },
            ),
            footer: const SizedBox.shrink(),
          ),
        ),
      ),
    );
  }

  /// Muestra el diálogo de edición de cotización
  void _showEditDialog(
    BuildContext context,
    SalesHomeController controller,
    QuotationRes quotation,
  ) {
    // Crear un formulario temporal para editar
    final quotationForm = QuotationReq(
      folio: quotation.folio,
      generalComment: quotation.generalComment,
      saleDate: quotation.saleDate,
      salesExecutives: quotation.salesExecutives,
    );

    // Cargar datos del cliente en el formulario
    quotationForm.customer.customerIdController.text = quotation.customerId;
    quotationForm.customer.fullNameController.text = quotation.customer.fullName;
    if (quotation.customer.email.isNotEmpty) {
      quotationForm.customer.emailController.text = quotation.customer.email;
    }
    if (quotation.customer.phoneNumber.isNotEmpty) {
      quotationForm.customer.phoneController.text = quotation.customer.phoneNumber;
    }

    // Cargar seguimientos existentes en el formulario
    if (quotation.followups.isNotEmpty) {
      // Limpiar seguimientos actuales del formulario
      for (var followup in quotationForm.followups) {
        followup.dispose();
      }
      quotationForm.followups.clear();

      // Agregar cada seguimiento existente
      for (final followup in quotation.followups) {
        quotationForm.addFollowup();
        final lastIndex = quotationForm.followups.length - 1;
        quotationForm.followups[lastIndex].commentController.text = followup.comment;
        quotationForm.followups[lastIndex].userIdController.text = followup.userId;
        // La fecha se puede actualizar si el modelo lo permite
      }
    }

    // Variable para controlar el auto-cierre
    bool shouldAutoClose = false;

    AppDialogWidget.show(
      context: context,
      title: 'Editar Cotización #${quotation.folio}',
      barrierDismissible: !controller.isUpdating.value,
      body: Obx(
        () {
          // Si terminó exitosamente y aún no se ha programado el auto-cierre
          if (!controller.isUpdating.value &&
              controller.errorMessage.value.isEmpty &&
              shouldAutoClose) {
            // Cerrar automáticamente después de 1 segundo
            Future.delayed(const Duration(milliseconds: 1000), () {
              if (context.mounted) {
                Navigator.pop(context);
                // Dispose después de cerrar el diálogo
                quotationForm.dispose();
              }
            });
          }

          // Si está actualizando, mostrar indicador de progreso
          if (controller.isUpdating.value) {
            return Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  CircularProgressIndicator(color: AppColors.primaryColor),
                  const SizedBox(height: 16),
                  const Text(
                    'Actualizando cotización...',
                    style: TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
                  ),
                ],
              ),
            );
          }

          // Si hubo éxito, mostrar confirmación
          if (controller.errorMessage.value.isEmpty && shouldAutoClose) {
            return Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Icon(Icons.check_circle, color: Colors.green, size: 48),
                  const SizedBox(height: 16),
                  const Text(
                    '¡Cotización actualizada correctamente!',
                    style: TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
                  ),
                ],
              ),
            );
          }

          // Mostrar formulario de edición
          return QuotationEditWidget(
            quotationModel: quotationForm,
            existingQuotation: quotation,
          );
        },
      ),
      footer: Obx(
        () {
          // Si está actualizando o ya terminó con éxito, no mostrar botones
          if (controller.isUpdating.value || (controller.errorMessage.value.isEmpty && shouldAutoClose)) {
            return const SizedBox.shrink();
          }

          // Mostrar botones de acción
          return Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                Expanded(
                  child: AppButton(
                    label: 'Cancelar',
                    colorType: 'secondary',
                    variant: 'outlined',
                    onPressed: () {
                      Navigator.pop(context);
                      // Dispose después de cerrar
                      Future.microtask(() => quotationForm.dispose());
                    },
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  flex: 2,
                  child: AppButton(
                    label: 'Guardar Cambios',
                    colorType: 'warning',
                    onPressed: () async {
                      // Comparar valores y crear map de cambios
                      final Map<String, dynamic> changes = {};

                      // Verificar folio
                      final newFolio = int.tryParse(quotationForm.folioController.text);
                      if (newFolio != null && newFolio != quotation.folio) {
                        changes['folio'] = newFolio;
                      }

                      // Verificar comentario general
                      final newComment = quotationForm.generalCommentController.text.trim();
                      if (newComment != quotation.generalComment) {
                        changes['generalComment'] = newComment;
                      }

                      // Verificar ejecutivos de venta
                      final newExecutives = quotationForm.salesExecutiveControllers
                          .map((c) => c.text.trim())
                          .where((t) => t.isNotEmpty)
                          .toList();
                      if (!_listEquals(newExecutives, quotation.salesExecutives)) {
                        changes['salesExecutives'] = newExecutives;
                      }

                      // Verificar fecha
                      if (quotationForm.saleDate != quotation.saleDate) {
                        changes['saleDate'] = quotationForm.saleDate;
                      }

                      // Verificar cliente
                      final newCustomerId = int.tryParse(quotationForm.customer.customerIdController.text.trim());
                      final originalCustomerId = int.tryParse(quotation.customerId);
                      if (newCustomerId != null && newCustomerId != originalCustomerId) {
                        changes['customerId'] = newCustomerId;
                        debugPrint('🔄 Cliente cambió de $originalCustomerId a $newCustomerId');
                      }

                      // Verificar seguimientos (followups)
                      if (quotationForm.followups.isNotEmpty) {
                        // Convertir los followups del formulario a DTOs
                        final newFollowups = quotationForm.followups.map((followup) {
                          return QuotationFollowupUpdateDto(
                            date: followup.date,
                            comment: followup.commentController.text.trim(),
                            userId: followup.userIdController.text.trim(),
                          );
                        }).toList();

                        // Solo agregar si hay seguimientos nuevos o diferentes
                        if (newFollowups.length != quotation.followups.length ||
                            _hasFollowupsChanged(newFollowups, quotation.followups)) {
                          changes['followups'] = newFollowups;
                          debugPrint('🔄 Seguimientos actualizados: ${newFollowups.length} items');
                        }
                      }

                      // Si no hay cambios, cerrar
                      if (changes.isEmpty) {
                        Navigator.pop(context);
                        // Dispose después de cerrar
                        Future.microtask(() => quotationForm.dispose());
                        NotificationService.showInfo('No hay cambios para guardar');
                        return;
                      }

                      debugPrint('📝 Cambios detectados: ${changes.keys.join(", ")}');

                      // Marcar para auto-cierre después de éxito
                      shouldAutoClose = true;

                      // Ejecutar actualización
                      await controller.updateQuotation(
                        originalQuotation: quotation,
                        updatedData: changes,
                      );

                      // NO llamar dispose aquí, se llama cuando el diálogo se cierra
                    },
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  /// Helper para comparar listas
  bool _listEquals<T>(List<T> a, List<T> b) {
    if (a.length != b.length) return false;
    for (int i = 0; i < a.length; i++) {
      if (a[i] != b[i]) return false;
    }
    return true;
  }

  /// Helper para verificar si los followups han cambiado
  bool _hasFollowupsChanged(List<QuotationFollowupUpdateDto> newFollowups, List<FollowupRes> originalFollowups) {
    if (newFollowups.length != originalFollowups.length) return true;

    for (int i = 0; i < newFollowups.length; i++) {
      final newF = newFollowups[i];
      final origF = originalFollowups[i];

      // Comparar los campos importantes
      if (newF.comment != origF.comment ||
          newF.userId != origF.userId ||
          !_isSameDate(newF.date, origF.date)) {
        return true;
      }
    }

    return false;
  }

  /// Helper para comparar fechas (solo la parte de fecha, ignorando tiempo)
  bool _isSameDate(DateTime a, DateTime b) {
    return a.year == b.year && a.month == b.month && a.day == b.day;
  }

  /// Muestra un diálogo de progreso y confirmación al cambiar el status
  void _showStatusChangeDialog(
    BuildContext context,
    SalesHomeController controller,
    QuotationRes quotation,
    String newStatusName,
  ) {
    // Convertir el nombre del status al valor numérico
    int newStatusValue = 1;
    switch (newStatusName.toLowerCase()) {
      case 'pendiente':
        newStatusValue = 1;
        break;
      case 'aceptado':
        newStatusValue = 2;
        break;
      case 'rechazado':
        newStatusValue = 3;
        break;
    }

    // Verificar si el status cambió
    int currentStatusValue = 1;
    if (quotation.status.toLowerCase() == 'aceptado') {
      currentStatusValue = 2;
    } else if (quotation.status.toLowerCase() == 'rechazado') {
      currentStatusValue = 3;
    }

    // Si el status no cambió, no hacer nada
    if (currentStatusValue == newStatusValue) {
      return;
    }

    // Variable para controlar el auto-cierre
    bool shouldAutoClose = false;

    // Mostrar diálogo de progreso
    AppDialogWidget.show(
      context: context,
      title: 'Actualizando Status',
      barrierDismissible: false, // No permitir cerrar mientras actualiza
      body: Obx(
        () {
          // Si terminó exitosamente y aún no se ha programado el auto-cierre
          if (!controller.isUpdating.value &&
              controller.errorMessage.value.isEmpty &&
              !shouldAutoClose) {
            shouldAutoClose = true;
            // Cerrar automáticamente después de 1.5 segundos
            Future.delayed(const Duration(milliseconds: 1500), () {
              if (context.mounted) {
                Navigator.pop(context);
              }
            });
          }

          return Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                if (controller.isUpdating.value) ...[
                  CircularProgressIndicator(color: AppColors.primaryColor),
                  const SizedBox(height: 16),
                  const Text(
                    'Actualizando status...',
                    style: TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
                  ),
                ] else ...[
                  Icon(
                    controller.errorMessage.value.isEmpty ? Icons.check_circle : Icons.error,
                    color: controller.errorMessage.value.isEmpty ? Colors.green : Colors.red,
                    size: 48,
                  ),
                  const SizedBox(height: 16),
                  Text(
                    controller.errorMessage.value.isEmpty
                        ? '¡Status actualizado correctamente!'
                        : 'Error al actualizar status',
                    style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
                  ),
                  if (controller.errorMessage.value.isNotEmpty) ...[
                    const SizedBox(height: 8),
                    Text(
                      controller.errorMessage.value,
                      style: const TextStyle(fontSize: 12, color: Colors.red),
                      textAlign: TextAlign.center,
                    ),
                  ],
                ],
                const SizedBox(height: 8),
                Text(
                  'Cotización #${quotation.folio}',
                  style: const TextStyle(fontSize: 12, color: Colors.grey),
                ),
                Text(
                  'Cliente: ${quotation.customer.fullName}',
                  style: const TextStyle(fontSize: 12, color: Colors.grey),
                ),
                const SizedBox(height: 8),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(
                      quotation.status,
                      style: TextStyle(
                        fontSize: 12,
                        color: StatusEnum.getColorByName(quotation.status),
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const Padding(
                      padding: EdgeInsets.symmetric(horizontal: 8.0),
                      child: Icon(Icons.arrow_forward, size: 16),
                    ),
                    Text(
                      newStatusName,
                      style: TextStyle(
                        fontSize: 12,
                        color: StatusEnum.getColorByName(newStatusName),
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          );
        },
      ),
      footer: Obx(
        () => controller.isUpdating.value
            ? const SizedBox.shrink()
            : controller.errorMessage.value.isNotEmpty
                ? Padding(
                    padding: const EdgeInsets.all(16),
                    child: AppButton(
                      label: 'Cerrar',
                      colorType: 'danger',
                      onPressed: () => Navigator.pop(context),
                    ),
                  )
                : const SizedBox.shrink(), // No mostrar botón si fue exitoso (se cierra auto)
      ),
    );

    // Ejecutar el cambio de status inmediatamente después de mostrar el diálogo
    Future.microtask(() async {
      await controller.updateQuotationStatus(quotation, newStatusValue);
      // El diálogo se actualizará automáticamente gracias a Obx
    });
  }

  void _showDeleteConfirmation(
    BuildContext context,
    SalesHomeController controller,
    QuotationRes quotation,
  ) {
    AppDialogWidget.show(
      context: context,
      title: 'Confirmar eliminación',
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Text(
          '¿Estás seguro de que deseas eliminar la cotización #${quotation.folio}?',
        ),
      ),
      footer: Row(
        mainAxisAlignment: MainAxisAlignment.end,
        children: [
          AppButton(
            label: 'Cancelar',
            variant: 'text',
            colorType: 'secondary',
            onPressed: () => Navigator.pop(context),
          ),
          Obx(
            () => AppButton(
              label: controller.isLoading.value ? 'Eliminando...' : 'Eliminar',
              variant: 'text',
              colorType: 'danger',
              onPressed: controller.isLoading.value
                  ? null
                  : () async {
                      Navigator.pop(context);
                      await controller.deleteQuotation(quotation);
                    },
            ),
          ),
        ],
      ),
    );
  }
}
