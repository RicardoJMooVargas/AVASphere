import 'package:get/get.dart';
import 'package:flutter/foundation.dart';
import 'package:vyaa_central_infor_webflutter/core/controllers/notification_services.dart';
import '../../../core/services/data/hive.service.dart';
import '../models/response/quotation_res.module.dart';
import '../models/requests/quotation_req.module.dart';
import '../models/requests/quotation_update_req.module.dart';
import '../services/api/quotation_manager.service.dart';

class SalesHomeController extends GetxController {
  final QuotationManagerService _quotationService = const QuotationManagerService();

  // Observable variables
  final RxList<QuotationRes> quotations = <QuotationRes>[].obs;
  final RxBool isLoading = false.obs;
  final RxBool isCreating = false.obs;
  final RxBool isUpdating = false.obs;
  final RxString errorMessage = ''.obs;

  // Form data
  final Rx<QuotationReq> currentQuotation = QuotationReq().obs;


  @override
  void onInit() {
    super.onInit();
    loadQuotations();
    _initializeDefaultSalesExecutive();
  }

  @override
  void onClose() {
    currentQuotation.value.dispose();
    super.onClose();
  }

  /// Cargar todas las cotizaciones
  Future<void> loadQuotations({
    DateTime? startDate,
    DateTime? endDate,
    String? customerName,
    int? folio,
  }) async {
    try {
      isLoading.value = true;
      errorMessage.value = '';

      final response = await _quotationService.getQuotations(
        startDate: startDate,
        endDate: endDate,
        customerName: customerName,
        folio: folio,
      );

      if (response.success && response.data != null) {
        quotations.value = response.data!;
        NotificationService.showSuccess('Cotizaciones cargadas correctamente');
      } else {
        errorMessage.value = response.message ?? 'Error desconocido';
        NotificationService.showError(response.message ?? 'Error al cargar cotizaciones');
      }
    } catch (e) {
      errorMessage.value = 'Error inesperado: $e';
      NotificationService.showError('Error inesperado al cargar cotizaciones');
    } finally {
      isLoading.value = false;
    }
  }

  /// Crear una nueva cotización
  Future<void> createQuotation() async {
    try {
      isCreating.value = true;
      errorMessage.value = '';

      // Validar que el formulario tenga datos básicos
      if (currentQuotation.value.customer.fullNameController.text.trim().isEmpty) {
        NotificationService.showWarning('El nombre del cliente es requerido');
        return;
      }

      // Validar que el folio sea válido
      if (currentQuotation.value.folioController.text.trim().isEmpty) {
        NotificationService.showWarning('El folio es requerido');
        return;
      }

      final response = await _quotationService.createQuotation(
        quotationData: currentQuotation.value,
      );

      if (response.success && response.data != null) {
        NotificationService.showSuccess('Cotización creada correctamente');

        // Agregar la nueva cotización a la lista
        quotations.add(response.data!);

        // Limpiar el formulario
        resetForm();

        // Recargar la lista para asegurar consistencia
        await loadQuotations();
      } else {
        errorMessage.value = response.message ?? 'Error desconocido';

        // Si es una advertencia (código 400), mostrar como warning
        if (response.isWarning) {
          NotificationService.showWarning(response.message ?? 'Advertencia al crear cotización');
        } else {
          // Si es un error crítico, mostrar como error
          NotificationService.showError(response.message ?? 'Error al crear cotización');
        }
      }
    } catch (e) {
      errorMessage.value = 'Error inesperado: $e';
      NotificationService.showError('Error inesperado al crear cotización');
    } finally {
      isCreating.value = false;
    }
  }

  /// Resetear el formulario
  void resetForm() {
    currentQuotation.value.dispose();
    currentQuotation.value = QuotationReq();

    // Inicializar ejecutivo de ventas por defecto después del reset
    _initializeDefaultSalesExecutive();
  }

  /// Inicializa el ejecutivo de ventas por defecto con el usuario actual
  Future<void> _initializeDefaultSalesExecutive() async {
    try {
      final userNameCache = await HiveService.getCache('user.name');
      final userName = userNameCache ?? '';

      if (userName.isNotEmpty) {
        // Si no hay ejecutivos o el primero está vacío, agregar el usuario actual
        if (currentQuotation.value.salesExecutiveControllers.isEmpty) {
          currentQuotation.value.addSalesExecutive();
        }

        if (currentQuotation.value.salesExecutiveControllers.isNotEmpty &&
            currentQuotation.value.salesExecutiveControllers[0].text.isEmpty) {
          currentQuotation.value.salesExecutiveControllers[0].text = userName;
          debugPrint('✅ Ejecutivo de ventas por defecto asignado: $userName');
        }
      }
    } catch (e) {
      debugPrint('❌ Error obteniendo usuario para ejecutivo por defecto: $e');
    }
  }

  /// Filtrar cotizaciones por texto
  List<QuotationRes> getFilteredQuotations(String searchText) {
    if (searchText.isEmpty) return quotations;
    
    return quotations.where((quotation) {
      final customerName = quotation.customer.fullName.toLowerCase();
      final folio = quotation.folio.toString();
      final status = quotation.status.toLowerCase();
      final search = searchText.toLowerCase();
      
      return customerName.contains(search) ||
             folio.contains(search) ||
             status.contains(search);
    }).toList();
  }

  /// Obtener estadísticas rápidas
  Map<String, int> getQuickStats() {
    final stats = <String, int>{
      'total': quotations.length,
      'pending': 0,
      'approved': 0,
      'rejected': 0,
    };

    for (final quotation in quotations) {
      final status = quotation.status.toLowerCase();
      if (status.contains('pending') || status.contains('pendiente')) {
        stats['pending'] = (stats['pending'] ?? 0) + 1;
      } else if (status.contains('approved') || status.contains('aprobado')) {
        stats['approved'] = (stats['approved'] ?? 0) + 1;
      } else if (status.contains('rejected') || status.contains('rechazado')) {
        stats['rejected'] = (stats['rejected'] ?? 0) + 1;
      }
    }

    return stats;
  }

  /// Refrescar datos
  Future<void> refreshData() async {
    await loadQuotations();
  }

  // Getter para verificar si está actualizando
  bool get isUpdatingValue => isUpdating.value;

  /// Actualiza solo el status de una cotización
  ///
  /// Parámetros:
  /// - [quotation]: La cotización a actualizar
  /// - [newStatus]: El nuevo status (1=Pendiente, 2=Aceptado, 3=Rechazado)
  Future<void> updateQuotationStatus(QuotationRes quotation, int newStatus) async {
    try {
      isUpdating.value = true;
      errorMessage.value = ''; // Limpiar errores anteriores

      debugPrint('🔄 Actualizando status de cotización ${quotation.folio} a $newStatus');

      // Crear el objeto de actualización solo con el status
      final updateData = QuotationUpdateReq(
        status: newStatus,
      );

      final response = await _quotationService.updateQuotation(
        idQuotation: int.tryParse(quotation.quotationId) ?? 0,
        quotationData: updateData,
      );

      // Manejar warnings (errores de negocio)
      if (response.isWarning) {
        errorMessage.value = response.message ?? 'No se pudo actualizar el status';
        NotificationService.showWarning(response.message ?? 'No se pudo actualizar el status');
        return;
      }

      if (response.success) {
        // No mostrar notificación de éxito aquí, el diálogo se encargará
        // NotificationService.showSuccess('Status actualizado correctamente');

        // Actualizar la cotización en la lista
        final index = quotations.indexWhere((q) => q.quotationId == quotation.quotationId);
        if (index != -1) {
          quotations[index] = response.data!;
        }

        // Recargar la lista para asegurar consistencia
        await loadQuotations();
      } else {
        errorMessage.value = response.message ?? 'Error desconocido';
        NotificationService.showError(response.message ?? 'Error al actualizar status');
      }
    } catch (e) {
      errorMessage.value = 'Error inesperado: $e';
      NotificationService.showError('Error inesperado al actualizar status');
      debugPrint('❌ Error actualizando status: $e');
    } finally {
      isUpdating.value = false;
    }
  }

  /// Actualiza una cotización existente con solo los campos modificados
  ///
  /// Parámetros:
  /// - [originalQuotation]: La cotización original antes de editar
  /// - [updatedData]: Map con los campos modificados y sus nuevos valores
  Future<void> updateQuotation({
    required QuotationRes originalQuotation,
    required Map<String, dynamic> updatedData,
  }) async {
    try {
      isUpdating.value = true;
      errorMessage.value = '';

      debugPrint('🔄 Actualizando cotización ${originalQuotation.folio}');
      debugPrint('📝 Campos a actualizar: ${updatedData.keys.join(", ")}');

      // Crear el objeto de actualización solo con los campos modificados
      final updateReq = QuotationUpdateReq(
        folio: updatedData['folio'] as int?,
        saleDate: updatedData['saleDate'] as DateTime?,
        status: updatedData['status'] as int?,
        generalComment: updatedData['generalComment'] as String?,
        customerId: updatedData['customerId'] as int?,
        salesExecutives: updatedData['salesExecutives'] as List<String>?,
        followups: updatedData['followups'] as List<QuotationFollowupUpdateDto>?,
        products: updatedData['products'] as List<QuotationProductUpdateDto>?,
        idConfigSys: updatedData['idConfigSys'] as int?,
      );

      final response = await _quotationService.updateQuotation(
        idQuotation: int.tryParse(originalQuotation.quotationId) ?? 0,
        quotationData: updateReq,
      );

      // Manejar warnings (errores de negocio)
      if (response.isWarning) {
        errorMessage.value = response.message ?? 'No se pudo actualizar la cotización';
        NotificationService.showWarning(response.message ?? 'No se pudo actualizar la cotización');
        return;
      }

      if (response.success) {
        // No mostrar notificación aquí, el diálogo se encargará
        // NotificationService.showSuccess('Cotización actualizada correctamente');

        // Actualizar la cotización en la lista
        final index = quotations.indexWhere((q) => q.quotationId == originalQuotation.quotationId);
        if (index != -1) {
          quotations[index] = response.data!;
        }

        // Recargar la lista para asegurar consistencia
        await loadQuotations();
      } else {
        errorMessage.value = response.message ?? 'Error desconocido';
        NotificationService.showError(response.message ?? 'Error al actualizar cotización');
      }
    } catch (e) {
      errorMessage.value = 'Error inesperado: $e';
      NotificationService.showError('Error inesperado al actualizar cotización');
      debugPrint('❌ Error actualizando cotización: $e');
    } finally {
      isUpdating.value = false;
    }
  }


  /// Eliminar una cotización
  Future<void> deleteQuotation(QuotationRes quotation) async {
    try {
      isLoading.value = true;
      errorMessage.value = '';

      debugPrint('🗑️ Eliminando cotización con ID: ${quotation.quotationId}');

      final response = await _quotationService.deleteQuotation(
        idQuotation: int.tryParse(quotation.quotationId) ?? 0,
      );

      if (response.success) {
        NotificationService.showSuccess('Cotización eliminada correctamente');

        // Remover la cotización de la lista local
        quotations.removeWhere((q) => q.quotationId == quotation.quotationId);


        debugPrint('✅ Cotización eliminada exitosamente');
      } else {
        errorMessage.value = response.message ?? 'Error desconocido al eliminar';
        NotificationService.showError(response.message ?? 'Error al eliminar cotización');
        debugPrint('❌ Error al eliminar cotización: ${response.message}');
      }
    } catch (e) {
      errorMessage.value = 'Error inesperado: $e';
      NotificationService.showError('Error inesperado al eliminar cotización');
      debugPrint('❌ Excepción al eliminar cotización: $e');
    } finally {
      isLoading.value = false;
    }
  }
}