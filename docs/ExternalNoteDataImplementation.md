# Implementación de Datos de Nota Externa en Sales

## Resumen

Se ha agregado un campo JSON opcional en la entidad `Sale` para almacenar datos de notas externas del sistema antiguo o externo.

## Cambios Realizados

### 1. Entidad `Sale`

**Archivo:** `src/AVASphere.ApplicationCore/Sales/Entities/Sale.cs`

#### Campo Agregado:
```csharp
[Column(TypeName = "jsonb")]
public ExternalNoteData? ExternalNoteData { get; set; }
```

#### Propiedad Calculada:
```csharp
[NotMapped]
public bool HasExternalNoteData => ExternalNoteData != null;
```

### 2. Clase `ExternalNoteData`

**Archivo:** `src/AVASphere.ApplicationCore/Sales/Entities/Sale.cs`

```csharp
public class ExternalNoteData
{
    public string Cliente { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Hora { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Caja { get; set; } = string.Empty;
    public string Zn { get; set; } = string.Empty;
    public string Nf { get; set; } = string.Empty;
    public string Agente { get; set; } = string.Empty;
    public string DireccionCliente { get; set; } = string.Empty;
    public string PoblacionCliente { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
    public string TelCliente { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public bool ExisteEnDB { get; set; }
}
```

### 3. Configuración Entity Framework

**Archivo:** `src/AVASphere.Infrastructure/Sales/Configuration/SaleEntitieConfig.cs`

```csharp
entity.Property(s => s.ExternalNoteData)
    .HasColumnName("ExternalNoteDataJson")
    .HasColumnType("jsonb")
    .IsRequired(false);
```

## Estructura JSON

### Ejemplo de Datos de Nota Externa:

```json
{
  "cliente": "006635",
  "nombreCliente": "EMILIANO  CABRIEL -",
  "folio": "002074",
  "fecha": "2025-11-04",
  "hora": "07:56:04",
  "serie": "CA",
  "caja": "12",
  "zn": "2 Foráneo",
  "nf": "N",
  "agente": "005 CAROLINA",
  "direccionCliente": "   ",
  "poblacionCliente": "   ",
  "emailCliente": "",
  "telCliente": "",
  "importe": 2981.36,
  "descuento": 524.85,
  "impuesto": 393.04,
  "total": 2849.55,
  "existeEnDB": true
}
```

## Casos de Uso

### Caso 1: Crear Venta con Datos de Nota Externa

```csharp
var sale = new Sale
{
    CustomerId = 1,
    SalesExecutive = "CAROLINA",
    Date = DateTime.Parse("2025-11-04"),
    Type = "External",
    Folio = "CA-002074",
    TotalAmount = 2849.55m,
    ExternalNoteData = new ExternalNoteData
    {
        Cliente = "006635",
        NombreCliente = "EMILIANO CABRIEL -",
        Folio = "002074",
        Fecha = "2025-11-04",
        Hora = "07:56:04",
        Serie = "CA",
        Caja = "12",
        Zn = "2 Foráneo",
        Nf = "N",
        Agente = "005 CAROLINA",
        DireccionCliente = "   ",
        PoblacionCliente = "   ",
        EmailCliente = "",
        TelCliente = "",
        Importe = 2981.36m,
        Descuento = 524.85m,
        Impuesto = 393.04m,
        Total = 2849.55m,
        ExisteEnDB = true
    }
};

await context.Sales.AddAsync(sale);
await context.SaveChangesAsync();
```

### Caso 2: Crear Venta sin Datos de Nota Externa

```csharp
var sale = new Sale
{
    CustomerId = 1,
    SalesExecutive = "Juan Perez",
    Date = DateTime.UtcNow,
    Type = "Regular",
    Folio = "V-00123",
    TotalAmount = 1500.00m,
    ExternalNoteData = null  // ✅ Opcional, puede ser null
};

await context.Sales.AddAsync(sale);
await context.SaveChangesAsync();
```

### Caso 3: Verificar si una Venta tiene Datos Externos

```csharp
var sale = await context.Sales
    .FirstOrDefaultAsync(s => s.SaleId == 1);

if (sale.HasExternalNoteData)
{
    Console.WriteLine($"Folio externo: {sale.ExternalNoteData.Folio}");
    Console.WriteLine($"Cliente: {sale.ExternalNoteData.NombreCliente}");
    Console.WriteLine($"Total: {sale.ExternalNoteData.Total}");
}
```

### Caso 4: Buscar Ventas con Datos Externos

```csharp
// Ventas que tienen datos de nota externa
var salesWithExternalData = await context.Sales
    .Where(s => s.ExternalNoteData != null)
    .ToListAsync();

// Ventas importadas de sistema externo que existen en DB
var existingExternalSales = await context.Sales
    .Where(s => s.ExternalNoteData != null && 
                s.ExternalNoteData.ExisteEnDB == true)
    .ToListAsync();
```

### Caso 5: Actualizar Datos Externos

```csharp
var sale = await context.Sales
    .FirstOrDefaultAsync(s => s.SaleId == 1);

if (sale != null)
{
    sale.ExternalNoteData = new ExternalNoteData
    {
        Cliente = "006635",
        NombreCliente = "EMILIANO CABRIEL - ACTUALIZADO",
        // ... otros campos
        ExisteEnDB = true
    };
    
    await context.SaveChangesAsync();
}
```

## Queries PostgreSQL

### Buscar por Folio Externo
```sql
SELECT * FROM "Sales"
WHERE "ExternalNoteDataJson"->>'Folio' = '002074';
```

### Buscar por Cliente Externo
```sql
SELECT * FROM "Sales"
WHERE "ExternalNoteDataJson"->>'Cliente' = '006635';
```

### Buscar por Serie
```sql
SELECT * FROM "Sales"
WHERE "ExternalNoteDataJson"->>'Serie' = 'CA';
```

### Buscar notas que existen en DB externa
```sql
SELECT * FROM "Sales"
WHERE ("ExternalNoteDataJson"->>'ExisteEnDB')::boolean = true;
```

### Obtener ventas con total mayor a cierto monto
```sql
SELECT * FROM "Sales"
WHERE ("ExternalNoteDataJson"->>'Total')::numeric > 1000;
```

## Queries LINQ

### Buscar por Folio Externo
```csharp
var sales = await context.Sales
    .Where(s => EF.Functions.JsonContains(
        s.ExternalNoteData, 
        new { Folio = "002074" }))
    .ToListAsync();
```

### Obtener estadísticas de ventas externas
```csharp
var stats = await context.Sales
    .Where(s => s.ExternalNoteData != null)
    .GroupBy(s => s.ExternalNoteData.Serie)
    .Select(g => new
    {
        Serie = g.Key,
        TotalVentas = g.Count(),
        MontoTotal = g.Sum(s => s.ExternalNoteData.Total),
        PromedioDescuento = g.Average(s => s.ExternalNoteData.Descuento)
    })
    .ToListAsync();
```

## Ventajas del Diseño

### ✅ 1. Trazabilidad Completa
- Mantiene todos los datos originales de la nota externa
- Permite auditoría y reconciliación con sistema externo

### ✅ 2. Flexibilidad
- Campo opcional: ventas normales no necesitan estos datos
- Puede almacenar datos de múltiples formatos/sistemas externos

### ✅ 3. Independencia
- Los datos externos no afectan el modelo principal
- Permite migración gradual sin perder información

### ✅ 4. Consultas Eficientes
- PostgreSQL JSONB permite indexar y consultar dentro del JSON
- Búsquedas rápidas por cualquier campo del JSON

## DTOs Recomendados

### CreateSaleFromExternalNoteDto
```csharp
public class CreateSaleFromExternalNoteDto
{
    public int CustomerId { get; set; }
    public ExternalNoteDataDto ExternalNoteData { get; set; }
}

public class ExternalNoteDataDto
{
    public string Cliente { get; set; }
    public string NombreCliente { get; set; }
    public string Folio { get; set; }
    public string Fecha { get; set; }
    public string Hora { get; set; }
    public string Serie { get; set; }
    public string Caja { get; set; }
    public string Zn { get; set; }
    public string Nf { get; set; }
    public string Agente { get; set; }
    public string DireccionCliente { get; set; }
    public string PoblacionCliente { get; set; }
    public string EmailCliente { get; set; }
    public string TelCliente { get; set; }
    public decimal Importe { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public bool ExisteEnDB { get; set; }
}
```

## Validaciones Recomendadas

```csharp
public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        // Si tiene datos externos, validar campos críticos
        When(s => s.ExternalNoteData != null, () =>
        {
            RuleFor(s => s.ExternalNoteData.Folio)
                .NotEmpty()
                .WithMessage("Folio externo es requerido");
                
            RuleFor(s => s.ExternalNoteData.Total)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total debe ser mayor o igual a 0");
                
            RuleFor(s => s.ExternalNoteData.Fecha)
                .NotEmpty()
                .WithMessage("Fecha externa es requerida");
                
            // Validar que el total de la venta coincida con el total externo
            RuleFor(s => s.TotalAmount)
                .Equal(s => s.ExternalNoteData.Total)
                .WithMessage("El total de la venta debe coincidir con el total de la nota externa");
        });
    }
}
```

## Servicio de Importación

```csharp
public class ExternalNoteImportService
{
    private readonly MasterDbContext _context;
    
    public async Task<Sale> ImportFromExternalNote(ExternalNoteDataDto noteData)
    {
        // Verificar si ya existe
        var existingSale = await _context.Sales
            .FirstOrDefaultAsync(s => 
                s.ExternalNoteData != null && 
                s.ExternalNoteData.Folio == noteData.Folio &&
                s.ExternalNoteData.Serie == noteData.Serie);
                
        if (existingSale != null)
        {
            throw new InvalidOperationException(
                $"Ya existe una venta con folio {noteData.Serie}-{noteData.Folio}");
        }
        
        // Crear nueva venta
        var sale = new Sale
        {
            SalesExecutive = noteData.Agente,
            Date = DateTime.Parse(noteData.Fecha),
            Type = "External",
            Folio = $"{noteData.Serie}-{noteData.Folio}",
            TotalAmount = noteData.Total,
            ExternalNoteData = new ExternalNoteData
            {
                Cliente = noteData.Cliente,
                NombreCliente = noteData.NombreCliente,
                Folio = noteData.Folio,
                Fecha = noteData.Fecha,
                Hora = noteData.Hora,
                Serie = noteData.Serie,
                Caja = noteData.Caja,
                Zn = noteData.Zn,
                Nf = noteData.Nf,
                Agente = noteData.Agente,
                DireccionCliente = noteData.DireccionCliente,
                PoblacionCliente = noteData.PoblacionCliente,
                EmailCliente = noteData.EmailCliente,
                TelCliente = noteData.TelCliente,
                Importe = noteData.Importe,
                Descuento = noteData.Descuento,
                Impuesto = noteData.Impuesto,
                Total = noteData.Total,
                ExisteEnDB = noteData.ExisteEnDB
            }
        };
        
        await _context.Sales.AddAsync(sale);
        await _context.SaveChangesAsync();
        
        return sale;
    }
}
```

## Endpoints API Recomendados

### POST /api/sales/import-external
Importar venta desde nota externa

### GET /api/sales/external/{folio}
Buscar venta por folio externo

### GET /api/sales/external/serie/{serie}
Buscar ventas por serie externa

### POST /api/sales/sync-external
Sincronizar con sistema externo

## Base de Datos

### Columna en PostgreSQL
```sql
ALTER TABLE "Sales"
ADD COLUMN "ExternalNoteDataJson" jsonb NULL;
```

### Índice para búsquedas rápidas
```sql
-- Índice para búsqueda por folio
CREATE INDEX idx_sales_external_folio 
ON "Sales" USING gin (("ExternalNoteDataJson"->'Folio'));

-- Índice para búsqueda por cliente
CREATE INDEX idx_sales_external_cliente 
ON "Sales" USING gin (("ExternalNoteDataJson"->'Cliente'));

-- Índice para búsqueda por serie
CREATE INDEX idx_sales_external_serie 
ON "Sales" USING gin (("ExternalNoteDataJson"->'Serie'));
```

## Migración

### Crear Migration
```bash
dotnet ef migrations add AddExternalNoteDataToSales --project AVASphere.Infrastructure --startup-project AVASphere.WebApi
```

### Aplicar Migration
```bash
dotnet ef database update --project AVASphere.Infrastructure --startup-project AVASphere.WebApi
```

---

**Fecha de Implementación:** 2025-11-04  
**Estado:** ✅ Completado  
**Campo:** `ExternalNoteData` (opcional, JSONB)

