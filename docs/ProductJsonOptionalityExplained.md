# Opcionalidad de Productos JSON en Ventas y Cotizaciones

## Resumen

Tanto `QuotationProductJson` como `SaleProductJson` son **completamente opcionales** en dos niveles:

### ✅ Nivel 1: La Lista Completa es Opcional
```csharp
public List<QuotationProductJson>? Products { get; set; }  // Puede ser null
public List<SaleProductJson>? Products { get; set; }        // Puede ser null
```

### ✅ Nivel 2: El ProductId dentro de cada Item es Opcional
```csharp
public class SaleProductJson
{
    public int? ProductId { get; set; }  // Puede ser null
    // ... otros campos requeridos
}
```

## Escenarios de Uso

### 📌 Escenario 1: Venta/Cotización SIN productos
```csharp
var sale = new Sale
{
    CustomerId = 1,
    Products = null  // ✅ Totalmente válido
};
```

### 📌 Escenario 2: Venta/Cotización CON productos vacíos
```csharp
var sale = new Sale
{
    CustomerId = 1,
    Products = new List<SaleProductJson>()  // ✅ Lista vacía
};
```

### 📌 Escenario 3: Productos SIN vincular a catálogo
```csharp
var sale = new Sale
{
    CustomerId = 1,
    Products = new List<SaleProductJson>
    {
        new SaleProductJson
        {
            ProductId = null,  // ✅ No vinculado a Product
            Quantity = 10,
            Description = "Servicio de instalación personalizado",
            UnitPrice = 500m,
            TotalPrice = 5000m,
            Unit = "Servicio"
        }
    }
};
```

### 📌 Escenario 4: Productos VINCULADOS a catálogo
```csharp
var sale = new Sale
{
    CustomerId = 1,
    Products = new List<SaleProductJson>
    {
        new SaleProductJson
        {
            ProductId = 45,  // ✅ Vinculado a Product.IdProduct = 45
            Quantity = 5,
            Description = "Perfil de Aluminio 2x3",
            UnitPrice = 150m,
            TotalPrice = 750m,
            Unit = "Metros"
        }
    }
};
```

### 📌 Escenario 5: Productos MIXTOS (vinculados y no vinculados)
```csharp
var sale = new Sale
{
    CustomerId = 1,
    Products = new List<SaleProductJson>
    {
        new SaleProductJson
        {
            ProductId = 45,  // Vinculado
            Quantity = 5,
            Description = "Perfil de Aluminio 2x3",
            UnitPrice = 150m,
            TotalPrice = 750m,
            Unit = "Metros"
        },
        new SaleProductJson
        {
            ProductId = null,  // NO vinculado
            Quantity = 1,
            Description = "Mano de obra especializada",
            UnitPrice = 2000m,
            TotalPrice = 2000m,
            Unit = "Servicio"
        }
    }
};
```

## Validaciones Recomendadas

### Validar si hay productos
```csharp
// Propiedad calculada ya incluida en la entidad
public bool HasProducts => Products?.Count > 0;

// Uso:
if (sale.HasProducts)
{
    // Procesar productos
}
```

### Validar si un producto está vinculado
```csharp
public bool IsProductLinked(SaleProductJson product)
{
    return product.ProductId.HasValue && product.ProductId.Value > 0;
}
```

### Obtener solo productos vinculados
```csharp
var linkedProducts = sale.Products?
    .Where(p => p.ProductId.HasValue)
    .ToList();
```

### Obtener solo productos NO vinculados
```csharp
var unlinkedProducts = sale.Products?
    .Where(p => !p.ProductId.HasValue)
    .ToList();
```

## Queries LINQ Comunes

### Ventas con productos
```csharp
var salesWithProducts = await context.Sales
    .Where(s => s.Products != null && s.Products.Count > 0)
    .ToListAsync();
```

### Ventas sin productos
```csharp
var salesWithoutProducts = await context.Sales
    .Where(s => s.Products == null || s.Products.Count == 0)
    .ToListAsync();
```

### Ventas con productos vinculados a un ProductId específico
```csharp
var salesWithProduct = await context.Sales
    .Where(s => s.Products != null && 
                s.Products.Any(p => p.ProductId == 45))
    .ToListAsync();
```

### Contar productos vendidos por tipo
```csharp
var productStats = await context.Sales
    .Where(s => s.Products != null)
    .SelectMany(s => s.Products!)
    .GroupBy(p => p.ProductId)
    .Select(g => new 
    {
        ProductId = g.Key,
        TotalQuantity = g.Sum(p => p.Quantity),
        TotalRevenue = g.Sum(p => p.TotalPrice)
    })
    .ToListAsync();
```

## Migración de Datos

### Inicializar productos existentes
Si ya tienes ventas/cotizaciones sin productos, puedes inicializarlas:

```csharp
// Opción 1: Dejar como null (preferido si no hay productos)
// No hacer nada, ya son null por defecto

// Opción 2: Inicializar como lista vacía
var sales = await context.Sales
    .Where(s => s.Products == null)
    .ToListAsync();

foreach (var sale in sales)
{
    sale.Products = new List<SaleProductJson>();
}

await context.SaveChangesAsync();
```

## Ventajas del Diseño Opcional

### ✅ 1. Flexibilidad Total
- Puedes crear ventas sin productos (servicios puros)
- Puedes agregar productos después
- Puedes mezclar productos vinculados y no vinculados

### ✅ 2. Migración Gradual
- Las ventas existentes no necesitan productos
- Puedes agregar productos solo cuando sea necesario
- No se rompe compatibilidad hacia atrás

### ✅ 3. Independencia de Catálogo
- No necesitas crear productos en el catálogo primero
- Útil para productos únicos o servicios personalizados
- Puedes vincular después si el producto se agrega al catálogo

### ✅ 4. Histórico Preservado
- Si un producto se elimina del catálogo, la venta mantiene la información en JSON
- No se pierden datos históricos
- Trazabilidad completa

## Ejemplo de API Controller

```csharp
[HttpPost]
public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto dto)
{
    var sale = new Sale
    {
        CustomerId = dto.CustomerId,
        SalesExecutive = dto.SalesExecutive,
        Date = DateTime.UtcNow,
        Type = dto.Type,
        
        // Productos opcionales
        Products = dto.Products?.Select(p => new SaleProductJson
        {
            ProductId = p.ProductId,  // Puede ser null
            Quantity = p.Quantity,
            Description = p.Description,
            UnitPrice = p.UnitPrice,
            TotalPrice = p.UnitPrice * (decimal)p.Quantity,
            Unit = p.Unit
        }).ToList(),
        
        TotalAmount = dto.Products?.Sum(p => p.UnitPrice * (decimal)p.Quantity) ?? 0
    };
    
    await _context.Sales.AddAsync(sale);
    await _context.SaveChangesAsync();
    
    return Ok(sale);
}
```

## Resumen Final

| Elemento | ¿Es Opcional? | Valor por Defecto | Uso |
|----------|---------------|-------------------|-----|
| `Products` (Lista) | ✅ SÍ (nullable) | `null` | Puede no tener productos |
| `ProductId` (int?) | ✅ SÍ (nullable) | `null` | Puede no estar vinculado |
| `Quantity` | ❌ NO | Requerido | Siempre necesario |
| `Description` | ❌ NO | `string.Empty` | Siempre necesario |
| `UnitPrice` | ❌ NO | Requerido | Siempre necesario |
| `TotalPrice` | ❌ NO | Requerido | Siempre necesario |
| `Unit` | ❌ NO | `string.Empty` | Siempre necesario |

**Conclusión:** Ambas estructuras son completamente opcionales y flexibles, permitiendo ventas/cotizaciones con o sin productos, y productos vinculados o no vinculados al catálogo.

