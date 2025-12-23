# 📋 Definición de Endpoints - Módulo de Ventas (Sales)

## 📌 Índice

1. [Quotation Manager](#quotation-manager)
2. [Quotation Version Manager](#quotation-version-manager)
3. [Sale Manager](#sale-manager)
4. [Sale Quotation Manager](#sale-quotation-manager)

---

## 🔖 Quotation Manager

Controlador principal para gestionar cotizaciones (creación, actualización, listado y eliminación).

### 1. **POST** `/api/QuotationManager/Register/Quotation`

**Propósito:** Crear una nueva cotización.

#### 📥 Request Body (CreateQuotationDto)

```json
{
  "folio": 12345,                        // Requerido: Número de folio de la cotización
  "saleDate": "2025-12-18",             // Opcional: Fecha de venta (DateOnly)
  "status": "Pending",                  // Opcional: "Pending", "Approved", "Rejected", "InProgress", "Completed"
  "generalComment": "Comentario general", // Opcional: Comentario sobre la cotización
  "customerId": 100,                    // Requerido: ID del cliente existente
  "newCustomers": [                     // Opcional: Lista de nuevos clientes a crear
    {
      "customerId": 0,
      "name": "Cliente Nuevo",
      "email": "cliente@example.com"
    }
  ],
  "salesExecutives": [                  // Opcional: Lista de ejecutivos de venta
    "Juan Pérez",
    "María García"
  ],
  "followups": [                        // Opcional: Seguimientos iniciales
    {
      "date": "2025-12-20T10:00:00Z",
      "comment": "Primera llamada de seguimiento",
      "userId": "usuario123"
    }
  ],
  "products": [                         // Opcional: Lista de productos
    {
      "productId": 1,                   // Opcional: ID del producto
      "quantity": 2.5,
      "description": "Producto ejemplo",
      "unitPrice": 100.00,
      "totalPrice": 250.00,
      "unit": "PZA"
    }
  ],
  "idConfigSys": 1                      // Opcional: ID de configuración del sistema (default: 0)
}
```

#### 📤 Response (201 Created)

```json
{
  "idQuotation": 456,
  "folio": 12345,
  "saleDate": "2025-12-18",
  "status": "Pending",
  "generalComment": "Comentario general",
  "customerId": 100,
  "salesExecutives": ["Juan Pérez", "María García"],
  "followups": [...],
  "products": [...],
  "createdAt": "2025-12-18T10:30:00Z",
  "updatedAt": "2025-12-18T10:30:00Z"
}
```

#### ❌ Response (400 Bad Request)

```json
{
  "error": "Validation failed: Folio is required",
  "type": "ValidationException"
}
```

---

### 2. **GET** `/api/QuotationManager/GetAll/Quotations`

**Propósito:** Obtener todas las cotizaciones con filtros opcionales.

#### 📥 Query Parameters (QuotationFilterDto)

```
?idQuotation=456           // Opcional: ID específico de cotización
&folio=12345               // Opcional: Filtrar por folio
&idCustomer=100            // Opcional: Filtrar por ID de cliente
&customerName=Juan         // Opcional: Filtrar por nombre de cliente
&startDate=2025-12-01      // Opcional: Fecha inicio (default: primer día del mes)
&endDate=2025-12-31        // Opcional: Fecha fin (default: último día del mes)
```

#### 📤 Response (200 OK)

```json
[
  {
    "idQuotation": 456,
    "folio": 12345,
    "saleDate": "2025-12-18",
    "status": "Pending",
    "generalComment": "Comentario",
    "customerId": 100,
    "customerName": "Juan Pérez",
    "salesExecutives": ["Juan Pérez"],
    "products": [...],
    "followups": [...],
    "totalAmount": 1500.00,
    "createdAt": "2025-12-18T10:30:00Z",
    "updatedAt": "2025-12-18T10:30:00Z"
  },
  {
    "idQuotation": 457,
    "folio": 12346,
    // ... más cotizaciones
  }
]
```

---

### 3. **PUT** `/api/QuotationManager/Update/{IdQuotation}`

**Propósito:** Actualizar una cotización existente (actualización parcial).

#### 📥 URL Parameter

```
IdQuotation: 456  // ID de la cotización a actualizar
```

#### 📥 Request Body (QuotationUpdateDto)

```json
{
  "folio": "12345-UPDATED",             // Opcional: Nuevo folio
  "saleDate": "2025-12-20",            // Opcional: Nueva fecha
  "status": "Approved",                // Opcional: Nuevo estado
  "generalComment": "Cotización aprobada", // Opcional: Nuevo comentario
  "salesExecutives": [                 // Opcional: Nuevos ejecutivos
    "Carlos López"
  ],
  "idConfigSys": 2                     // Opcional: Nueva configuración
}
```

**Nota:** Solo se actualizan los campos enviados (no nulos).

#### 📤 Response (200 OK)

```json
{
  "idQuotation": 456,
  "folio": "12345-UPDATED",
  "saleDate": "2025-12-20",
  "status": "Approved",
  "generalComment": "Cotización aprobada",
  "salesExecutives": ["Carlos López"],
  "updatedAt": "2025-12-18T11:00:00Z"
}
```

#### ❌ Response (404 Not Found)

```json
"Quotation not found"
```

---

### 4. **DELETE** `/api/QuotationManager/Delete/IdQuotation`

**Propósito:** Eliminar una cotización.

#### 📥 Query Parameter

```
?IdQuotation=456
```

#### 📤 Response (204 No Content)

Sin contenido (eliminación exitosa).

#### ❌ Response (404 Not Found)

```json
"Failed to delete quotation."
```

#### ❌ Response (409 Conflict)

```json
"Cannot delete quotation: it is linked to a sale."
```

---

### 5. **DELETE** `/api/QuotationManager/Delete/IdFollowupsJson`

**Propósito:** Eliminar un seguimiento específico de una cotización.

#### 📥 Query Parameters

```
?IdQuotation=456
&IdFollowupsJson=10
```

#### 📤 Response (204 No Content)

Sin contenido (eliminación exitosa).

#### ❌ Response (404 Not Found)

```json
"Failed to delete followup."
```

---

## 🔖 Quotation Version Manager

Controlador para gestionar versiones de cotizaciones (historial y control de cambios).

### 1. **GET** `/api/QuotationVersionManager/GetQuotationVersionById`

**Propósito:** Obtener una versión específica de cotización por su ID.

#### 📥 Query Parameter

```
?IdQuotationVersion=20
```

#### 📤 Response (200 OK)

```json
{
  "idQuotationVersion": 20,
  "idQuotation": 456,
  "versionNumber": 3,
  "folio": "12345-V3",
  "products": [...],
  "totalAmount": 1500.00,
  "createdAt": "2025-12-18T10:30:00Z",
  "createdBy": "usuario123",
  "changeComment": "Se actualizó el precio del producto X"
}
```

#### ❌ Response (404 Not Found)

Sin contenido.

---

### 2. **GET** `/api/QuotationVersionManager/GetAllQuotationVersions`

**Propósito:** Listar todas las versiones de una cotización específica.

#### 📥 Query Parameter

```
?IdQuotation=456
```

#### 📤 Response (200 OK)

```json
[
  {
    "idQuotationVersion": 18,
    "idQuotation": 456,
    "versionNumber": 1,
    "totalAmount": 1200.00,
    "createdAt": "2025-12-15T10:00:00Z",
    "createdBy": "usuario123"
  },
  {
    "idQuotationVersion": 19,
    "idQuotation": 456,
    "versionNumber": 2,
    "totalAmount": 1350.00,
    "createdAt": "2025-12-16T14:30:00Z",
    "createdBy": "usuario456"
  },
  {
    "idQuotationVersion": 20,
    "idQuotation": 456,
    "versionNumber": 3,
    "totalAmount": 1500.00,
    "createdAt": "2025-12-18T10:30:00Z",
    "createdBy": "usuario123"
  }
]
```

---

### 3. **GET** `/api/QuotationVersionManager/GetLatestQuotationVersion`

**Propósito:** Obtener la versión más reciente de una cotización.

#### 📥 Query Parameter

```
?IdQuotation=456
```

#### 📤 Response (200 OK)

```json
{
  "idQuotationVersion": 20,
  "idQuotation": 456,
  "versionNumber": 3,
  "folio": "12345-V3",
  "products": [...],
  "totalAmount": 1500.00,
  "createdAt": "2025-12-18T10:30:00Z",
  "createdBy": "usuario123",
  "changeComment": "Última actualización de precios"
}
```

#### ❌ Response (404 Not Found)

Sin contenido.

---

## 🔖 Sale Manager

Controlador principal para gestionar ventas (consultas externas, creación, eliminación).

### 1. **GET** `/api/SaleManager/GetSalesExternal`

**Propósito:** Consultar ventas del sistema externo InforAVA, combinadas con datos internos.

**Funcionalidades:**
- Búsqueda inteligente (números → folio, texto → cliente)
- Filtros avanzados (monto, satisfacción, vinculación)
- Identificación de ventas no importadas
- Paginación

#### 📥 Query Parameters (SaleFilterDto)

```
?fecha=2025-12-18              // Opcional: Fecha a consultar (default: hoy)
&search=12345                  // Opcional: Búsqueda inteligente (folio o cliente)
&customerName=ARMALUM          // Opcional: Nombre del cliente
&folio=A-12345                 // Opcional: Folio específico
&isLinked=false                // Opcional: true=vinculadas, false=no vinculadas, null=todas
&minAmount=1000                // Opcional: Monto mínimo
&maxAmount=5000                // Opcional: Monto máximo
&satisfactionLevel=4           // Opcional: Nivel de satisfacción (0-5)
&limit=100                     // Opcional: Límite de resultados (default: 100)
&offset=0                      // Opcional: Offset para paginación (default: 0)
```

#### 📤 Response (200 OK)

```json
{
  "success": true,
  "catalogo": "AVA01",
  "fecha": "2025-12-18",
  "filtrosAplicados": {
    "search": "12345",
    "customerName": null,
    "folio": null,
    "isLinked": false,
    "montoMin": 1000,
    "montoMax": 5000,
    "satisfactionLevel": null
  },
  "paginacion": {
    "limit": 100,
    "offset": 0,
    "totalResultados": 15
  },
  "resumen": {
    "totalVentas": 15,
    "ventasVinculadas": 5,
    "ventasNoVinculadas": 10
  },
  "datos": [
    {
      "nf": "001",
      "caja": "01",
      "serie": "A",
      "folio": "12345",
      "fecha": "2025-12-18",
      "hora": "10:30:00",
      "zn": "1 Local",
      "cliente": "C001",
      "nombreCliente": "ARMALUM SA DE CV",
      "agente": "Juan Pérez",
      "total": 2500.00,
      "isLinked": false,
      "internalSaleId": null,
      "satisfactionLevel": null
    },
    // ... más ventas
  ]
}
```

#### ❌ Response (400 Bad Request)

```json
{
  "success": false,
  "error": "Invalid date format"
}
```

#### ❌ Response (502 Bad Gateway)

```json
{
  "success": false,
  "error": "El sistema externo InforAVA no está disponible.",
  "detalles": "Connection timeout"
}
```

---

### 2. **GET** `/api/SaleManager/VerifyExternalConnection`

**Propósito:** Verificar la disponibilidad del sistema externo InforAVA.

#### 📥 Request

Sin parámetros.

#### 📤 Response (200 OK)

```json
{
  "success": true,
  "available": true,
  "mensaje": "Sistema externo InforAVA disponible."
}
```

#### 📤 Response (Sistema no disponible)

```json
{
  "success": true,
  "available": false,
  "mensaje": "Sistema externo InforAVA no disponible."
}
```

#### ❌ Response (502 Bad Gateway)

```json
{
  "success": false,
  "available": false,
  "mensaje": "No se pudo verificar la conectividad.",
  "detalles": "Connection refused"
}
```

---

### 3. **POST** `/api/SaleManager/CreateSale`

**Propósito:** Crear una venta desde datos externos.

#### 📥 Request Body (SaleExternalDto)

```json
{
  "folio": "A-12345",
  "fecha": "2025-12-18",
  "hora": "10:30:00",
  "codeClient": "100",                  // Se convertirá a customerId
  "nombreCliente": "Cliente Ejemplo",
  "agente": "Juan Pérez",
  "total": 2500.00,
  "nf": "001",
  "caja": "01",
  "serie": "A",
  "products": [
    {
      "productId": 1,
      "quantity": 2,
      "description": "Producto A",
      "unitPrice": 1000.00,
      "totalPrice": 2000.00,
      "unit": "PZA"
    }
  ]
}
```

#### 📤 Response (201 Created)

```json
{
  "idSale": 789,
  "folio": "A-12345",
  "totalAmount": 2500.00,
  "saleDate": "2025-12-18T10:30:00Z",
  "type": "External",
  "linkedQuotationCount": 0,
  "productCount": 1
}
```

---

### 4. **DELETE** `/api/SaleManager/DeleteIdSale`

**Propósito:** Eliminar una venta.

#### 📥 Query Parameter

```
?id=789
```

#### 📤 Response (204 No Content)

Sin contenido (eliminación exitosa).

#### ❌ Response (404 Not Found)

Sin contenido.

---

### 5. **POST** `/api/SaleManager/CreateFromQuotations`

**Propósito:** Crear una venta desde una o varias cotizaciones existentes.

#### 📥 Request Body (CreateSaleFromQuotationsDto)

```json
{
  "quotationIds": [456, 457],           // Requerido: IDs de cotizaciones a vincular
  "salesExecutive": "Juan Pérez",       // Requerido: Ejecutivo de venta
  "date": "2025-12-18T10:30:00Z",      // Opcional: Fecha de venta (default: ahora)
  "type": "Internal",                   // Opcional: Tipo de venta
  "customerId": 100,                    // Requerido: ID del cliente
  "folio": "V-2025-001",               // Opcional: Folio de la venta
  "totalAmount": 3500.00,              // Requerido: Monto total
  "deliveryDriver": "Pedro García",     // Opcional: Conductor de entrega
  "homeDelivery": true,                 // Opcional: ¿Entrega a domicilio? (default: false)
  "deliveryDate": "2025-12-20T14:00:00Z", // Opcional: Fecha de entrega
  "satisfactionLevel": 5,               // Opcional: Nivel de satisfacción (0-5)
  "satisfactionReason": "Excelente servicio", // Opcional: Razón de satisfacción
  "comment": "Cliente frecuente",       // Opcional: Comentarios
  "afterSalesFollowupDate": "2025-12-25T10:00:00Z", // Opcional: Fecha de seguimiento
  "idConfigSys": 1                      // Opcional: ID de configuración (default: 0)
}
```

#### 📤 Response (201 Created)

```json
{
  "idSale": 790,
  "folio": "V-2025-001",
  "totalAmount": 3500.00,
  "saleDate": "2025-12-18T10:30:00Z",
  "type": "Internal",
  "linkedQuotationCount": 2,
  "productCount": 5
}
```

---

### 6. **GET** `/api/SaleManager/ObtenerVentasPorFecha`

**Propósito:** Obtener ventas directamente de la API externa por fecha (método legacy).

#### 📥 Query Parameters

```
?fecha=2025-12-18              // Requerido: Fecha en formato YYYY-MM-DD
&folio=A-12345                 // Opcional: Filtrar por folio
&nombreCliente=ARMALUM         // Opcional: Filtrar por nombre de cliente
&cliente=C001                  // Opcional: Filtrar por código de cliente
```

#### 📤 Response (200 OK)

```json
[
  {
    "folio": "A-12345",
    "fecha": "2025-12-18",
    "hora": "10:30:00",
    "nombreCliente": "ARMALUM SA DE CV",
    "cliente": "C001",
    "agente": "Juan Pérez",
    "total": 2500.00,
    "nf": "001",
    "caja": "01",
    "serie": "A"
  }
]
```

#### ❌ Response (404 Not Found)

```json
"No se encontraron ventas para la fecha especificada."
```

---

### 7. **GET** `/api/SaleManager/External-Detail`

**Propósito:** Obtener el detalle de productos de una venta externa específica.

#### 📥 Query Parameters

```
?nf=001                        // Requerido: Número fiscal
&caja=01                       // Requerido: Número de caja
&serie=A                       // Requerido: Serie
&folio=12345                   // Requerido: Folio
```

#### 📤 Response (200 OK)

```json
{
  "success": true,
  "identificadores": {
    "nf": "001",
    "caja": "01",
    "serie": "A",
    "folio": "12345"
  },
  "totalProductos": 3,
  "datos": [
    {
      "codigo": "PROD-001",
      "descripcion": "Producto A",
      "cantidad": 2,
      "precioUnitario": 1000.00,
      "precioTotal": 2000.00,
      "unidad": "PZA",
      "descuento": 0.00
    },
    {
      "codigo": "PROD-002",
      "descripcion": "Producto B",
      "cantidad": 1,
      "precioUnitario": 500.00,
      "precioTotal": 500.00,
      "unidad": "PZA",
      "descuento": 0.00
    }
  ]
}
```

#### ❌ Response (400 Bad Request)

```json
{
  "success": false,
  "error": "Los parámetros 'NF', 'Caja', 'Serie' y 'Folio' son obligatorios."
}
```

#### ❌ Response (502 Bad Gateway)

```json
{
  "success": false,
  "error": "El sistema externo InforAVA no está disponible.",
  "detalles": "Connection timeout"
}
```

---

## 🔖 Sale Quotation Manager

Controlador para gestionar relaciones entre ventas y cotizaciones (vinculación N:N).

### 1. **POST** `/api/SaleQuotationManager/MarkPrimary`

**Propósito:** Marcar una cotización como primaria para una venta.

**Funcionalidad:**
- Desmarca la cotización primaria anterior (si existe)
- Marca la nueva cotización como primaria

#### 📥 Request Body (MarkPrimaryRequest)

```json
{
  "idSale": 790,
  "idQuotation": 456
}
```

#### 📤 Response (204 No Content)

Sin contenido (operación exitosa).

#### ❌ Response (404 Not Found)

```json
"Sale or quotation not found, or operation could not be completed."
```

---

### 2. **POST** `/api/SaleQuotationManager/InsertAndSaleQuotationExternal`

**Propósito:** Insertar una venta desde el sistema externo y vincularla automáticamente con una cotización.

**Flujo:**
1. Obtener datos de la venta desde InforAVA
2. Obtener detalles de productos
3. Registrar la venta internamente
4. Vincular con la cotización especificada
5. Opcionalmente marcar como primaria

#### 📥 Request Body (InsertExternalSaleAndQuotationRequest)

```json
{
  "catalogo": "AVA01",                  // Requerido: Catálogo InforAVA
  "folio": "12345",                     // Requerido: Folio de la venta
  "caja": "01",                         // Requerido: Número de caja
  "serie": "A",                         // Requerido: Serie
  "nf": "001",                          // Opcional: Número fiscal
  "idQuotation": 456,                   // Requerido: ID de cotización a vincular
  "markAsPrimary": true,                // Opcional: Marcar como primaria (default: false)
  "generalComment": "Venta confirmada" // Opcional: Comentario general
}
```

#### 📤 Response (201 Created)

```json
{
  "idSale": 791,
  "folio": "12345",
  "totalAmount": 2500.00,
  "saleDate": "2025-12-18T10:30:00Z",
  "type": "External",
  "linkedQuotationCount": 1,
  "productCount": 3
}
```

#### ❌ Response (409 Conflict)

```json
{
  "message": "Sale with this folio already exists.",
  "saleId": 791
}
```

#### ❌ Response (404 Not Found)

```json
{
  "message": "Quotation with ID 456 not found."
}
```

---

### 3. **GET** `/api/SaleQuotationManager/GetAllBySale`

**Propósito:** Obtener todas las cotizaciones vinculadas a una venta.

#### 📥 Query Parameter

```
?IdSale=790
```

#### 📤 Response (200 OK)

```json
[
  {
    "idSaleQuotation": 50,
    "idQuotation": 456,
    "idSale": 790,
    "createdAt": "2025-12-18T10:30:00Z",
    "createdBy": "usuario123",
    "isPrimary": true,
    "productsJson": [
      {
        "productId": 1,
        "quantity": 2,
        "description": "Producto A",
        "unitPrice": 1000.00,
        "totalPrice": 2000.00,
        "unit": "PZA"
      }
    ],
    "priceSnapshot": {
      "subtotal": 2000.00,
      "tax": 320.00,
      "total": 2320.00
    },
    "generalComment": "Primera cotización vinculada"
  },
  {
    "idSaleQuotation": 51,
    "idQuotation": 457,
    "idSale": 790,
    "createdAt": "2025-12-18T11:00:00Z",
    "createdBy": "usuario456",
    "isPrimary": false,
    "productsJson": [...],
    "priceSnapshot": {...},
    "generalComment": "Segunda cotización vinculada"
  }
]
```

#### ❌ Response (404 Not Found)

```json
{
  "message": "No quotations found for this sale."
}
```

---

### 4. **GET** `/api/SaleQuotationManager/GetRelationship`

**Propósito:** Obtener los detalles de una relación específica entre venta y cotización.

#### 📥 Query Parameters

```
?IdSale=790
&IdQuotation=456
```

#### 📤 Response (200 OK)

```json
{
  "idSaleQuotation": 50,
  "idQuotation": 456,
  "idSale": 790,
  "createdAt": "2025-12-18T10:30:00Z",
  "createdBy": "usuario123",
  "isPrimary": true,
  "productsJson": [...],
  "priceSnapshot": {
    "subtotal": 2000.00,
    "tax": 320.00,
    "total": 2320.00
  },
  "generalComment": "Cotización primaria"
}
```

#### ❌ Response (404 Not Found)

```json
{
  "message": "Relationship between sale and quotation not found."
}
```

---

### 5. **PUT** `/api/SaleQuotationManager/ManageRelationship`

**Propósito:** Gestionar relaciones complejas entre ventas y cotizaciones.

**Operaciones disponibles:**
- **DELETE**: Eliminar solo la relación (venta permanece)
- **DELETE_WITH_SALE**: Eliminar relación Y venta (cascada)
- **REASSIGN**: Reasignar cotización a otra venta

#### 📥 Request Body (ManageSaleQuotationRelationshipRequest)

**Ejemplo 1: Eliminar relación (venta permanece)**
```json
{
  "idSale": 790,
  "idQuotation": 456,
  "operation": "DELETE",
  "reason": "Relación incorrecta"
}
```

**Ejemplo 2: Eliminar relación y venta**
```json
{
  "idSale": 790,
  "idQuotation": 456,
  "operation": "DELETE_WITH_SALE",
  "confirmDeletionWithSale": true,      // Requerido para confirmar
  "reason": "Venta duplicada"
}
```

**Ejemplo 3: Reasignar cotización a otra venta**
```json
{
  "idSale": 790,
  "idQuotation": 456,
  "operation": "REASSIGN",
  "idNewSale": 791,                     // Requerido: Nueva venta destino
  "reason": "Corrección de folio"
}
```

#### 📤 Response (200 OK)

```json
{
  "success": true,
  "operation": "REASSIGN",
  "message": "Quotation 456 reassigned from Sale 790 to Sale 791",
  "idSale": 790,
  "idQuotation": 456,
  "idNewSale": 791,
  "timestamp": "2025-12-18T11:30:00Z"
}
```

#### ❌ Response (400 Bad Request)

```json
{
  "message": "Invalid operation. Must be one of: DELETE, DELETE_WITH_SALE, REASSIGN"
}
```

**Validaciones específicas:**

```json
// DELETE_WITH_SALE sin confirmación
{
  "message": "DELETE_WITH_SALE requires ConfirmDeletionWithSale = true to prevent accidental deletions."
}

// REASSIGN sin IdNewSale
{
  "message": "REASSIGN operation requires IdNewSale to be specified and greater than 0."
}

// REASSIGN con IdNewSale igual a IdSale
{
  "message": "IdNewSale cannot be the same as IdSale."
}
```

---

### 6. **DELETE** `/api/SaleQuotationManager/DeleteRelationship` ⚠️ DEPRECATED

**Propósito:** Desvincular una cotización de una venta.

**⚠️ ADVERTENCIA:** Este endpoint está deprecado. Usar `ManageRelationship` con `operation="DELETE"` en su lugar.

#### 📥 Query Parameters

```
?IdSale=790
&IdQuotation=456
```

#### 📤 Response (204 No Content)

Sin contenido (eliminación exitosa).

#### ❌ Response (404 Not Found)

Sin contenido.

---

## 📊 Estructuras Comunes

### SingleProductJson

```json
{
  "productId": 1,                       // Opcional: ID del producto real
  "quantity": 2.5,                      // Requerido: Cantidad
  "description": "Producto ejemplo",    // Requerido: Descripción
  "unitPrice": 100.00,                  // Requerido: Precio unitario
  "totalPrice": 250.00,                 // Requerido: Precio total
  "unit": "PZA"                         // Requerido: Unidad de medida
}
```

### StatusEnum (Quotation)

Valores posibles:
- `"Pending"` - Pendiente
- `"Approved"` - Aprobada
- `"Rejected"` - Rechazada
- `"InProgress"` - En progreso
- `"Completed"` - Completada

### PriceSnapshotJson

```json
{
  "subtotal": 2000.00,
  "tax": 320.00,
  "discount": 0.00,
  "total": 2320.00
}
```

---

## 🔍 Notas Importantes

### Búsqueda Inteligente (Search)

El parámetro `search` en `/api/SaleManager/GetSalesExternal` funciona de manera inteligente:

- **Solo números** (ej: `"12345"`): Busca por folio
- **Solo texto** (ej: `"ARMALUM"`): Busca por nombre de cliente
- **Texto con números** (ej: `"CLIENTE-001"`): Busca por nombre de cliente

### Vinculación de Ventas y Cotizaciones

- Una venta puede tener múltiples cotizaciones vinculadas (N:N)
- Solo una cotización puede ser marcada como "primaria" por venta
- Al marcar una nueva como primaria, se desmarca automáticamente la anterior

### Sistema Externo (InforAVA)

- Catálogo fijo: `"AVA01"`
- API Base: `http://apivaa.ddns.net:8080/api/rest/tsm/`
- Endpoints externos:
  - `VENTASPORFECHAV?CATALOGO={catalogo}&FECHA={fecha}` - Lista de ventas
  - `DetalleVentaV?NF={nf}&CAJA={caja}&SERIE={serie}&FOLIO={folio}` - Detalle de productos

### Códigos de Estado HTTP

- **200 OK**: Operación exitosa
- **201 Created**: Recurso creado exitosamente
- **204 No Content**: Operación exitosa sin contenido de respuesta
- **400 Bad Request**: Error de validación o parámetros incorrectos
- **404 Not Found**: Recurso no encontrado
- **409 Conflict**: Conflicto (ej: folio duplicado, relación existente)
- **502 Bad Gateway**: Sistema externo no disponible

---

## 📝 Changelog

- **2025-12-18**: Documentación inicial de endpoints de Sales
- Incluye: Quotation Manager, Quotation Version Manager, Sale Manager, Sale Quotation Manager

---

**Versión del documento:** 1.0  
**Última actualización:** 2025-12-18  
**Autor:** Sistema AVASphere

