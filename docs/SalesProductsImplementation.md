# Implementación de Productos en Ventas y Cotizaciones

## Resumen de Cambios

Se han realizado las siguientes mejoras en el sistema de ventas y cotizaciones para incluir soporte de productos con JSON simplificado y relaciones opcionales.

## Cambios Realizados

### 1. Entidad `Sale` (Ventas)

**Archivo:** `src/AVASphere.ApplicationCore/Sales/Entities/Sale.cs`

#### Campos Agregados:
- **`Products`** (List<SaleProductJson>): Lista JSONB con productos simplificados
  - `ProductId` (int?): Referencia opcional al producto real
  - `Quantity` (double): Cantidad
  - `Description` (string): Descripción del producto
  - `UnitPrice` (decimal): Precio unitario
  - `TotalPrice` (decimal): Precio total
  - `Unit` (string): Unidad de medida

#### Propiedades Calculadas:
- **`HasProducts`**: Indica si la venta tiene productos asociados

### 2. Entidad `Quotation` (Cotizaciones)

**Archivo:** `src/AVASphere.ApplicationCore/Sales/Entities/Quotation.cs`

#### Campos Agregados:
- **`Products`** (List<QuotationProductJson>): Lista JSONB con productos simplificados
  - Misma estructura que SaleProductJson
  - Permite cotizar antes de vincular productos reales

#### Propiedades Calculadas:
- **`HasProducts`**: Indica si la cotización tiene productos asociados

### 3. Configuración de Entity Framework

#### `SaleEntitieConfig.cs`
**Archivo:** `src/AVASphere.Infrastructure/Sales/Configuration/SaleEntitieConfig.cs`

```csharp
entity.Property(s => s.Products)
    .HasColumnName("ProductsJson")
    .HasColumnType("jsonb")
    .HasDefaultValueSql("'[]'::jsonb");
```

#### `QuotationEntitieConfig.cs`
**Archivo:** `src/AVASphere.Infrastructure/Sales/Configuration/QuotationEntititeConfig.cs`

```csharp
entity.Property(q => q.Products)
    .HasColumnName("ProductsJson")
    .HasColumnType("jsonb")
    .HasDefaultValueSql("'[]'::jsonb");
```

### 4. Relaciones Actualizadas

#### `ConfigSys`
**Archivo:** `src/AVASphere.ApplicationCore/Common/Entities/General/ConfigSys.cs`

- ✅ Activada la colección `Sales` (anteriormente comentada)

```csharp
public ICollection<Sale> Sales { get; set; } = new List<Sale>();
```

#### `Customer`
**Archivo:** `src/AVASphere.ApplicationCore/Common/Entities/General/Customer.cs`

- ✅ Agregada la colección `Sales`

```csharp
public List<Sale> Sales { get; set; } = new List<Sale>();
```

## Modelo de Datos

### Diagrama de Relaciones

```
ConfigSys (1) ----< (N) Sales
ConfigSys (1) ----< (N) Quotations
Customer (1) ----< (N) Sales
Customer (1) ----< (N) Quotations
```

### JSON de Productos

#### Estructura de `SaleProductJson` / `QuotationProductJson`:

```json
[
  {
    "productId": 123,           // Opcional - Referencia al producto real
    "quantity": 10.5,            // Cantidad
    "description": "Perfil de Aluminio 2x3", // Descripción
    "unitPrice": 150.00,         // Precio unitario
    "totalPrice": 1575.00,       // Precio total
    "unit": "Metros"             // Unidad de medida
  },
  {
    "productId": null,           // Sin vincular a producto real
    "quantity": 5,
    "description": "Servicio de Instalación",
    "unitPrice": 500.00,
    "totalPrice": 2500.00,
    "unit": "Servicio"
  }
]
```

## Ventajas del Diseño

### 1. **Flexibilidad**
- Las ventas y cotizaciones pueden tener productos JSON sin necesidad de vincularlos a la tabla Products
- Útil para productos temporales, servicios o items personalizados

### 2. **Trazabilidad**
- El campo `ProductId` opcional permite vincular productos reales cuando existan
- Mantiene histórico incluso si el producto se elimina o modifica

### 3. **Simplicidad**
- JSON simplificado con solo los campos necesarios para ventas
- Evita joins complejos en consultas frecuentes

### 4. **Migración Gradual**
- Se puede iniciar con JSON puro
- Posteriormente vincular productos reales según se vayan creando

## Casos de Uso

### Caso 1: Venta con Productos JSON (Sin Productos en Catálogo)
```csharp
var sale = new Sale
{
    CustomerId = 1,
    Products = new List<SaleProductJson>
    {
        new SaleProductJson
        {
            ProductId = null, // Sin vincular
            Quantity = 10,
            Description = "Perfil especial bajo pedido",
            UnitPrice = 200,
            TotalPrice = 2000,
            Unit = "Metros"
        }
    }
};
```

### Caso 2: Venta con Productos Vinculados
```csharp
var sale = new Sale
{
    CustomerId = 1,
    Products = new List<SaleProductJson>
    {
        new SaleProductJson
        {
            ProductId = 45, // Vinculado a producto real
            Quantity = 5,
            Description = "Perfil 2x3 6061",
            UnitPrice = 150,
            TotalPrice = 750,
            Unit = "Metros"
        }
    }
};
```

### Caso 3: Cotización que se Convierte en Venta
```csharp
// 1. Crear cotización con productos
var quotation = new Quotation
{
    CustomerId = 1,
    Products = new List<QuotationProductJson> { /* productos */ }
};

// 2. Al convertir a venta, copiar productos
var sale = new Sale
{
    CustomerId = quotation.CustomerId,
    Products = quotation.Products.Select(p => new SaleProductJson
    {
        ProductId = p.ProductId,
        Quantity = p.Quantity,
        Description = p.Description,
        UnitPrice = p.UnitPrice,
        TotalPrice = p.TotalPrice,
        Unit = p.Unit
    }).ToList(),
    LinkedQuotations = new List<QuotationReference>
    {
        new QuotationReference
        {
            QuotationId = quotation.QuotationId,
            QuotationFolio = quotation.Folio
        }
    }
};
```

## Consideraciones Técnicas

### Base de Datos (PostgreSQL)
- Los campos JSON usan tipo `jsonb` para mejor rendimiento
- Se puede indexar y consultar dentro del JSON si es necesario
- Default value: `'[]'::jsonb` (array vacío)

### Migraciones
Al ejecutar migraciones, se agregarán las columnas:
- `Sales.ProductsJson` (jsonb)
- `Quotations.ProductsJson` (jsonb)

### Queries LINQ
```csharp
// Obtener ventas con productos
var salesWithProducts = await context.Sales
    .Where(s => s.Products.Count > 0)
    .ToListAsync();

// Calcular total de productos vendidos
var totalProducts = sales
    .SelectMany(s => s.Products)
    .Sum(p => p.Quantity);
```

## Próximos Pasos Sugeridos

1. **Crear Migration**
   ```bash
   dotnet ef migrations add AddProductsJsonToSalesAndQuotations
   ```

2. **Implementar Servicios**
   - Servicio para agregar productos a ventas
   - Servicio para copiar productos de cotización a venta
   - Validaciones de precios y cantidades

3. **Implementar DTOs**
   - `CreateSaleDto` con lista de productos
   - `UpdateSaleDto` para modificar productos
   - Similar para Quotations

4. **Endpoints API**
   - POST `/api/sales` - Crear venta con productos
   - PUT `/api/sales/{id}/products` - Actualizar productos
   - GET `/api/sales/{id}/products` - Obtener productos de venta

## Validaciones Recomendadas

```csharp
// Validar que TotalPrice = UnitPrice * Quantity
public bool ValidateProductPrices()
{
    return Products.All(p => 
        Math.Abs(p.TotalPrice - (p.UnitPrice * (decimal)p.Quantity)) < 0.01m
    );
}

// Validar que TotalAmount coincida con suma de productos
public bool ValidateTotalAmount()
{
    var productsTotal = Products.Sum(p => p.TotalPrice);
    return Math.Abs(TotalAmount - productsTotal) < 0.01m;
}
```

---

**Fecha de Implementación:** 2025-11-04  
**Estado:** ✅ Completado

