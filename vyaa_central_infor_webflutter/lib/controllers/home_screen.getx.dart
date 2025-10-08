import 'package:get/get.dart';
import 'package:vyaa_central_infor_webflutter/core/internalControllers/notification_services.dart';
import '../models/responses/quotation_res.module.dart';
import '../models/requests/quotation_req.module.dart';
import '../services/api/quotation_manager.service.dart';

class HomeScreenController extends GetxController {
  final QuotationManagerService _quotationService = QuotationManagerService();

  // Observable variables
  final RxList<QuotationRes> quotations = <QuotationRes>[].obs;
  final RxBool isLoading = false.obs;
  final RxBool isCreating = false.obs;
  final RxString errorMessage = ''.obs;

  // Form data
  final Rx<QuotationReq> currentQuotation = QuotationReq().obs;

  @override
  void onInit() {
    super.onInit();
    loadQuotations();
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

      if (response.isSuccess && response.data != null) {
        quotations.value = response.data!;
        NotificationService.showSuccess('Cotizaciones cargadas correctamente');
      } else {
        errorMessage.value = response.error ?? 'Error desconocido';
        NotificationService.showError(response.error ?? 'Error al cargar cotizaciones');
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
        quotationReq: currentQuotation.value,
      );

      if (response.isSuccess && response.data != null) {
        NotificationService.showSuccess('Cotización creada correctamente');
        
        // Agregar la nueva cotización a la lista
        quotations.add(response.data!);
        
        // Limpiar el formulario
        resetForm();
        
        // Recargar la lista para asegurar consistencia
        await loadQuotations();
      } else {
        errorMessage.value = response.error ?? 'Error desconocido';
        NotificationService.showError(response.error ?? 'Error al crear cotización');
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
}