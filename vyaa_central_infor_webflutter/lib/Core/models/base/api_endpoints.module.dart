import 'package:vyaa_central_infor_webflutter/configs/enums.dart';

// Definición de tipos para mapeo
typedef ResponseMapper<T> = T Function(dynamic data);
typedef RequestMapper<T> = Map<String, dynamic> Function(T model);
class ApiEndpoint<TRequest, TResponse> {
  final String path;
  final HttpMethod method;
  final bool requiresAuth;
  final bool useBody;
  final bool useQuery;
  final List<String> urlParams;
  
  // Nuevos campos para mapeo
  final ResponseMapper<TResponse>? responseMapper;
  final RequestMapper<TRequest>? requestMapper;

  const ApiEndpoint({
    required this.path,
    required this.method,
    this.requiresAuth = true,
    this.useBody = false,
    this.useQuery = false,
    this.urlParams = const [],
    this.responseMapper,
    this.requestMapper,
  });
}