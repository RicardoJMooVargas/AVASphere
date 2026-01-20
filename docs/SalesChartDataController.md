# Sales Chart Data Controller - Mapeo de Endpoints

## Información General
**Controlador:** SalesChartDataController  
**Ruta base:** `api/SalesChartData`  
**Grupo:** Sales  

---

## 1. Sales Summary (Resumen de Ventas)

### Endpoint
```
POST /api/SalesChartData/sales-summary
```

### Request (filterSalesByCostReq)
```json
{
  "type": 0|1|2,
  "customerName": "string (opcional)",
  "productName": "string (opcional)",
  "specificDate": "2024-01-15T00:00:00Z (para type=1 daily)",
  "year": 2024,
  "month": 1,
  "startDate": "2024-01-01T00:00:00Z (para type=0 personalized)",
  "endDate": "2024-01-31T23:59:59Z (para type=0 personalized)"
}
```

### Response (salesSummaryRes)
```json
{
  "totalAmount": 15000.50,
  "totalSalesCount": 25,
  "details": [
    {
      "period": "2024-01-15",
      "amount": 2500.00,
      "salesCount": 5,
      "customerName": "",
      "productName": ""
    },
    {
      "period": "2024-01-16",
      "amount": 3200.00,
      "salesCount": 7,
      "customerName": "",
      "productName": ""
    }
  ],
  "metadata": {
    "type": 1,
    "startDate": null,
    "endDate": null,
    "year": 2024,
    "month": 1,
    "specificDate": "2024-01-15T00:00:00Z",
    "appliedFilters": ["Cliente: Empresa ABC", "Producto: Producto A"]
  }
}
```

---

## 2. Sales By Agent (Ventas por Agente)

### Endpoint
```
POST /api/SalesChartData/sales-by-agent
```

### Request (filterSalesByAgentReq)
```json
{
  "type": 0|1|2,
  "salesAgent": "string (opcional)",
  "customerName": "string (opcional)",
  "specificDate": "2024-01-15T00:00:00Z (para type=1 daily)",
  "year": 2024,
  "month": 1,
  "startDate": "2024-01-01T00:00:00Z (para type=0 personalized)",
  "endDate": "2024-01-31T23:59:59Z (para type=0 personalized)"
}
```

### Response (salesByAgentRes)
```json
{
  "agents": [
    {
      "agentName": "014 CANDY",
      "totalAmount": 8500.00,
      "salesCount": 12,
      "customerDetails": [
        {
          "customerName": "OMAR CHIQUINI  981 8",
          "amount": 3500.00,
          "salesCount": 4
        },
        {
          "customerName": "Empresa ABC",
          "amount": 5000.00,
          "salesCount": 8
        }
      ]
    },
    {
      "agentName": "015 MARIA",
      "totalAmount": 6500.50,
      "salesCount": 13,
      "customerDetails": []
    }
  ],
  "totalAmount": 15000.50,
  "metadata": {
    "type": 2,
    "startDate": null,
    "endDate": null,
    "year": 2024,
    "month": 1,
    "specificDate": null,
    "appliedFilters": ["Agente: 014 CANDY", "Cliente: OMAR CHIQUINI"]
  }
}
```

---

## 3. Sales By Product (Ventas por Producto)

### Endpoint
```
POST /api/SalesChartData/sales-by-product
```

### Request (filterSalesByProductReq)
```json
{
  "type": 0|1|2,
  "productName": "string (opcional)",
  "specificDate": "2024-01-15T00:00:00Z (para type=1 daily)",
  "year": 2024,
  "month": 1,
  "startDate": "2024-01-01T00:00:00Z (para type=0 personalized)",
  "endDate": "2024-01-31T23:59:59Z (para type=0 personalized)"
}
```

### Response (salesByProductRes)
```json
{
  "products": [
    {
      "productName": "Tubo PVC 4 pulgadas",
      "totalQuantity": 50.0,
      "totalAmount": 5000.00,
      "salesCount": 8,
      "periodQuantities": [
        {
          "period": "2024-01-15",
          "quantity": 10.0,
          "amount": 1000.00
        },
        {
          "period": "2024-01-16",
          "quantity": 15.0,
          "amount": 1500.00
        }
      ]
    },
    {
      "productName": "Codo PVC 90 grados",
      "totalQuantity": 25.0,
      "totalAmount": 1250.00,
      "salesCount": 5,
      "periodQuantities": [
        {
          "period": "2024-01-15",
          "quantity": 25.0,
          "amount": 1250.00
        }
      ]
    }
  ],
  "metadata": {
    "type": 0,
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "year": null,
    "month": null,
    "specificDate": null,
    "appliedFilters": ["Producto: Tubo PVC"]
  },
  "topFrequencies": [
    {
      "period": "2024-01-15",
      "frequency": 15,
      "productName": "Tubo PVC 4 pulgadas"
    },
    {
      "period": "2024-01-16",
      "frequency": 12,
      "productName": "Codo PVC 90 grados"
    }
  ]
}
```

---

## Tipos de Filtro (TypeFilter)

| Valor | Nombre | Descripción | Campos Requeridos |
|-------|--------|-------------|-------------------|
| `0` | `personalized` | Filtro por rango de fechas personalizado | `startDate`, `endDate` |
| `1` | `daily` | Filtro por día específico | `specificDate` |
| `2` | `monthly` | Filtro por año y mes | `year`, `month` |

---

## Comportamiento de Filtros

### Filtros Básicos (1er Nivel)
- **type**: Define el tipo de agrupación temporal
- **specificDate**: Usado solo cuando `type = 1` (daily)
- **year/month**: Usados solo cuando `type = 2` (monthly)
- **startDate/endDate**: Usados solo cuando `type = 0` (personalized)

### Filtros Avanzados (2do Nivel)
- **customerName**: Filtra por nombre de cliente (búsqueda parcial)
- **productName**: Filtra por nombre de producto (búsqueda parcial)
- **salesAgent**: Filtra por agente de ventas específico

---

## Agrupación de Datos

### Sales Summary
- **daily**: Agrupa por día individual
- **monthly**: Agrupa por mes
- **personalized**: Agrupa por día dentro del rango especificado

### Sales By Agent
- Agrupa por agente de ventas
- Si se especifica `customerName`, incluye detalles por cliente para cada agente
- Busca agentes tanto en `SalesExecutive` como en `AuxNoteDataJson.Agente`

### Sales By Product
- Agrupa por nombre de producto
- Incluye cantidades y montos por período
- Calcula frecuencias más altas (top 10)
- Extrae productos del campo JSON `ProductsJson`

---

## Códigos de Respuesta

| Código | Descripción |
|--------|-------------|
| **200 OK** | Solicitud exitosa |
| **500 Internal Server Error** | Error interno del servidor |

---

## Ejemplos de Uso

### Obtener ventas del día específico
```json
POST /api/SalesChartData/sales-summary
{
  "type": 1,
  "specificDate": "2024-01-15T00:00:00Z"
}
```

### Obtener ventas por agente en un mes
```json
POST /api/SalesChartData/sales-by-agent
{
  "type": 2,
  "year": 2024,
  "month": 1,
  "salesAgent": "014 CANDY"
}
```

### Obtener productos vendidos en rango personalizado
```json
POST /api/SalesChartData/sales-by-product
{
  "type": 0,
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-31T23:59:59Z",
  "productName": "PVC"
}
```
