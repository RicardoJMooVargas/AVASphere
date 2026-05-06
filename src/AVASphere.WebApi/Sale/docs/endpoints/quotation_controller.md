# QuotationManagerController

## GET api/QuotationManager/GetAll/Quotations

### Entrada de datos

- **Tipo**: parámetros por **URL (query string)**.
- **Parámetros**:
  - `startDate` (DateTime?, opcional): fecha inicial del rango.
  - `endDate` (DateTime?, opcional): fecha final del rango.
  - `filter` (QuotationFilterDto?, opcional): filtros adicionales enviados por query.
    - `IdQuotation` (int?, opcional)
    - `Folio` (int?, opcional)
    - `IdCustomer` (int?, opcional)
    - `CustomerName` (string?, opcional)
    - `ExternalId` (int?, opcional)
    - `SalesExecutive` (string?, opcional)
    - `StartDate` (DateTime?, opcional)
    - `EndDate` (DateTime?, opcional)

### Reglas por defecto

- Si **no** se envía ningún valor para `startDate`, `endDate`, `filter.StartDate` ni `filter.EndDate`:
  - `startDate` se establece al **primer día del mes actual**.
  - `endDate` se establece al **día de hoy**.

### Salida de datos

**200 OK** (lista de cotizaciones):

```json
[
  {
    "idQuotation": 120,
    "folio": 20240012,
    "saleDate": "2024-11-05",
    "status": "Pending",
    "generalComment": "Cotización inicial",
    "customer": {
      "idCustomer": 45,
      "externalId": 1001,
      "name": "Juan",
      "lastName": "Pérez",
      "phoneNumber": "+52 555 555 5555",
      "email": "juan.perez@email.com",
      "taxId": "XAXX010101000",
      "direction": null,
      "settings": null,
      "paymentMethods": null,
      "paymentTerms": null,
      "fullName": "Juan Pérez"
    },
    "salesExecutives": ["vendedor1"],
    "followups": [],
    "products": null,
    "linkedSaleId": null,
    "linkedSaleFolio": null,
    "isLinkedToSale": false,
    "createdAt": "2024-11-05T15:30:00Z",
    "updatedAt": "2024-11-05T15:30:00Z",
    "idConfigSys": 1
  }
]
```

### Errores posibles

- **400 BadRequest**: `{ "error": "<mensaje>", "type": "<tipo>" }`
