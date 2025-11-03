﻿# Sistema de Auto-incremento de Índices en JSON - Customer

## Descripción General

El servicio `CustomerService` implementa un sistema automático de asignación de índices para los campos JSON de la entidad `Customer`. Esto garantiza que cada JSON tenga un índice único y secuencial.

## Tipos de JSON con Auto-incremento

### 1. SettingsCustomerJson
- **Comportamiento**: Se auto-incrementa solo si se proporciona el objeto
- **Uso**: Configuraciones específicas del cliente (ruta, tipo, descuento)

### 2. DirectionJson
- **Comportamiento**: Siempre se auto-incrementa (campo requerido)
- **Uso**: Información de dirección del cliente

### 3. PaymentMethodsJson
- **Comportamiento**: Se auto-incrementa solo si se proporciona el objeto
- **Uso**: Métodos de pago del cliente (código, descripción, banco, etc.)

### 4. PaymentTermsJson
- **Comportamiento**: Se auto-incrementa solo si se proporciona el objeto
- **Uso**: Términos de pago del cliente (tipo, fecha expiración, moneda)

## Funcionamiento

### Para Crear (POST /api/common/customer/create)
```csharp
// Los índices se asignan automáticamente - transparente al usuario
var request = new CustomerCreateRequest
{
    ExternalId = 12345,
    Name = "Juan",
    LastName = "Pérez",
    // DTOs simplificados sin Index - el sistema lo maneja internamente
    Settings = new CustomerSettingsDto
    {
        Type = "General",
        Discount = 10.0
    },
    Direction = new CustomerDirectionDto
    {
        City = "México",
        Colony = "Centro"
    }
};
```

### Para Actualizar (PUT /api/common/customer/update)
```csharp
// Auto-incremento completamente transparente
var request = new CustomerUpdateRequest
{
    IdCustomer = 1,
    // Los índices se generan automáticamente - sin exposición al usuario
    Settings = new CustomerSettingsDto
    {
        Type = "Premium",
        Discount = 15.0
    },
    Direction = new CustomerDirectionDto
    {
        City = "Guadalajara",
        Colony = "Chapultepec"
    }
};
```

## Implementación Técnica

### Arquitectura de 3 Capas

**1. Repositorio (`CustomerRepository`)** - Acceso a datos optimizado:
```csharp
public async Task<int> GetNextIndexForSettingsAsync()
public async Task<int> GetNextIndexForDirectionAsync()
public async Task<int> GetNextIndexForPaymentMethodsAsync()
public async Task<int> GetNextIndexForPaymentTermsAsync()
```

**2. Servicio (`CustomerService`)** - Lógica de negocio:
- Coordina las llamadas al repositorio
- Aplica reglas de auto-incremento condicional

**3. Controlador (`CustomerController`)** - Exposición de API REST

### Lógica de Asignación

1. **Creación**: Todos los índices se calculan y asignan automáticamente
2. **Actualización**: Solo se auto-incrementa si `Index == 0`
3. **Cálculo**: Consultas LINQ optimizadas que EF Core traduce a SQL eficiente

## ¿Dónde Implementar el Auto-incremento?

### 🎯 **RECOMENDADO: Repositorio** (Implementación actual)
- ✅ **Separación clara**: Lógica de datos en la capa correcta
- ✅ **Reutilizable**: Otros servicios pueden usar los mismos métodos
- ✅ **Optimización**: Consultas LINQ eficientes traducidas a SQL
- ✅ **Mantenible**: Fácil de testear y modificar

### 🏃 **Alternativa: Base de Datos**
```sql
-- Función PostgreSQL para auto-incremento
CREATE OR REPLACE FUNCTION get_next_customer_settings_index()
RETURNS INTEGER AS $$
BEGIN
    RETURN COALESCE(
        (SELECT MAX(CAST(SettingsCustomerJson->>'Index' AS INTEGER)) + 1 
         FROM common.Customers 
         WHERE SettingsCustomerJson IS NOT NULL), 
        1
    );
END;
$$ LANGUAGE plpgsql;
```
- ✅ **Rendimiento máximo**: Ejecución directa en BD
- ❌ **Específico de PostgreSQL**: Menos portabilidad
- ❌ **Mantenimiento**: Requiere migraciones para cambios

### ❌ **No Recomendado: Solo en Servicio**
- ❌ **Responsabilidad mixta**: Servicio hace consultas de índices
- ❌ **Menos eficiente**: Múltiples round-trips a BD

## Ventajas del Sistema

1. **Transparencia Total**: Los índices son completamente invisibles para el usuario de la API
2. **Unicidad**: Garantiza índices únicos sin duplicados
3. **Secuencial**: Los índices siguen un orden consecutivo automáticamente
4. **Simplicidad**: DTOs limpios sin campos técnicos expuestos
5. **Automatización**: Reduce errores manuales en asignación de índices
6. **Optimización**: Consultas EF Core eficientemente traducidas a SQL
7. **Experiencia de Usuario**: API más limpia y fácil de usar

## Ejemplos de Uso

### Crear Cliente Básico
```json
POST /api/common/customer/create
{
  "externalId": 12345,
  "name": "María",
  "lastName": "González",
  "phoneNumber": 5551234567,
  "email": "maria@example.com",
  "settings": {
    "type": "General",
    "discount": 5.0
  }
}
```

**Resultado**: Los índices se asignan automáticamente de forma transparente.

### Actualizar Cliente
```json
PUT /api/common/customer/update
{
  "idCustomer": 1,
  "settings": {
    "type": "VIP",
    "discount": 20.0
  }
}
```

**Resultado**: Los índices se manejan internamente - transparente al usuario.

### Actualizar con Método de Pago
```json
PUT /api/common/customer/update
{
  "idCustomer": 1,
  "paymentMethod": {
    "code": "CC",
    "description": "Credit Card",
    "bank": "BBVA"
  }
}
```

**Resultado**: Los índices se asignan automáticamente sin intervención del usuario.
