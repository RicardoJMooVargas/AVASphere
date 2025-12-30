# Implementación del Endpoint GET Principal de Projects

## Resumen

Este documento detalla la implementación completa del endpoint GET principal de Projects que devuelve clientes con sus proyectos asociados, incluyendo filtros opcionales.

## Características Implementadas

✅ **Endpoint GET** en `ProjectController`  
✅ **Filtros dinámicos**: IdCustomer, CurrentHito, CategoryIds  
✅ **Respuesta jerárquica**: Clientes → Proyectos → Cotizaciones y Categorías  
✅ **Consultas optimizadas** con Entity Framework Core (Include/ThenInclude)  
✅ **Arquitectura en capas**: Controller → Service → Repository  
✅ **DTOs específicos** para la respuesta  
✅ **Manejo de errores** básico  

---

## 1. DTOs (ApplicationCore/Projects/DTOs/ProjectDTOs.cs)

### DTOs Agregados

```csharp
// DTOs for GET main endpoint with Customers and Projects
public class CustomerWithProjectsResponseDto
{
    public int IdCustomer { get; set; }
    public int ExternalId { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TaxId { get; set; }
    public SettingsCustomerJsonDto? SettingsCustomerJson { get; set; }
    public List<ProjectWithDetailsResponseDto> Projects { get; set; } = new List<ProjectWithDetailsResponseDto>();
}

public class ProjectWithDetailsResponseDto
{
    public int IdProject { get; set; }
    public Hitos CurrentHito { get; set; }
    public string? WrittenAddress { get; set; }
    public string? ExactAddress { get; set; }
    public AppointmentJson? AppointmentJson { get; set; }
    public ICollection<VisitsJson>? VisitsJson { get; set; }
    public ProjectQuoteResponseDto? ProjectQuote { get; set; }
    public List<ListOfCategoriesResponseDto> ListOfCategories { get; set; } = new List<ListOfCategoriesResponseDto>();
}

public class ProjectQuoteResponseDto
{
    public int IdProjectQuotes { get; set; }
    public double GrandTotal { get; set; }
    public double TotalTaxes { get; set; }
}

public class ListOfCategoriesResponseDto
{
    public int IdListOfCategories { get; set; }
    public ProjectCategoryResponseDto? ProjectCategory { get; set; }
}

public class SettingsCustomerJsonDto
{
    public int Index { get; set; }
    public string? Route { get; set; }
    public string? Type { get; set; }
    public double Discount { get; set; }
}

// Filter DTO for main GET endpoint
public class GetProjectsWithCustomersFilterDto
{
    public int? IdCustomer { get; set; }
    public Hitos? CurrentHito { get; set; }
    public List<int>? CategoryIds { get; set; }
}
```

**Nota**: `ProjectCategoryResponseDto` ya existe en `ProjectCategoryDTOs.cs` y se reutiliza.

---

## 2. Interfaz del Repositorio (ApplicationCore/Projects/Interfaces/IProjectRepository.cs)

### Método Agregado

```csharp
// Main GET endpoint - Get customers with their projects and filters
Task<IEnumerable<Project>> GetProjectsWithFiltersAsync(
    int? idCustomer = null, 
    int? currentHito = null, 
    List<int>? categoryIds = null);
```

---

## 3. Implementación del Repositorio (Infrastructure/Projects/Repository/ProjectRepository.cs)

### Método Agregado

```csharp
// Main GET endpoint - Get projects with filters and full relations
public async Task<IEnumerable<Project>> GetProjectsWithFiltersAsync(
    int? idCustomer = null, 
    int? currentHito = null, 
    List<int>? categoryIds = null)
{
    var query = _context.Set<Project>()
        .Include(p => p.Customer)
        .Include(p => p.ProjectQuote)
        .Include(p => p.ListOfCategories)
            .ThenInclude(lc => lc.ProjectCategory)
        .AsNoTracking()
        .AsQueryable();

    // Apply filters dynamically
    if (idCustomer.HasValue)
    {
        query = query.Where(p => p.IdCustomer == idCustomer.Value);
    }

    if (currentHito.HasValue)
    {
        query = query.Where(p => (int)p.CurrentHito == currentHito.Value);
    }

    if (categoryIds != null && categoryIds.Any())
    {
        query = query.Where(p => p.ListOfCategories.Any(lc => categoryIds.Contains(lc.IdProjectCategory)));
    }

    return await query.ToListAsync();
}
```

---

## 4. Interfaz del Servicio (ApplicationCore/Projects/Interfaces/IProjectService.cs)

### Método Agregado

```csharp
// Main GET endpoint - Get customers with their projects
Task<IEnumerable<CustomerWithProjectsResponseDto>> GetCustomersWithProjectsAsync(
    GetProjectsWithCustomersFilterDto? filter = null);
```

---

## 5. Implementación del Servicio (Infrastructure/Projects/Services/ProjectService.cs)

### Método Agregado

```csharp
// Main GET endpoint - Get customers with their projects
public async Task<IEnumerable<CustomerWithProjectsResponseDto>> GetCustomersWithProjectsAsync(
    GetProjectsWithCustomersFilterDto? filter = null)
{
    // Get projects with filters
    var projects = await _repository.GetProjectsWithFiltersAsync(
        filter?.IdCustomer,
        filter?.CurrentHito.HasValue == true ? (int?)filter.CurrentHito : null,
        filter?.CategoryIds
    );

    // Group projects by customer
    var customerGroups = projects
        .Where(p => p.Customer != null)
        .GroupBy(p => p.Customer!)
        .Select(g => new CustomerWithProjectsResponseDto
        {
            IdCustomer = g.Key.IdCustomer,
            ExternalId = g.Key.ExternalId,
            Name = g.Key.Name,
            LastName = g.Key.LastName,
            Email = g.Key.Email,
            PhoneNumber = g.Key.PhoneNumber,
            TaxId = g.Key.TaxId,
            SettingsCustomerJson = g.Key.SettingsCustomerJson != null ? new SettingsCustomerJsonDto
            {
                Index = g.Key.SettingsCustomerJson.Index,
                Route = g.Key.SettingsCustomerJson.Route,
                Type = g.Key.SettingsCustomerJson.Type,
                Discount = g.Key.SettingsCustomerJson.Discount
            } : null,
            Projects = g.Select(p => new ProjectWithDetailsResponseDto
            {
                IdProject = p.IdProject,
                CurrentHito = p.CurrentHito,
                WrittenAddress = p.WrittenAddress,
                ExactAddress = p.ExactAddress,
                AppointmentJson = p.AppointmentJson,
                VisitsJson = p.VisitsJson,
                ProjectQuote = p.ProjectQuote != null ? new ProjectQuoteResponseDto
                {
                    IdProjectQuotes = p.ProjectQuote.IdProjectQuotes,
                    GrandTotal = p.ProjectQuote.GrandTotal,
                    TotalTaxes = p.ProjectQuote.TotalTaxes
                } : null,
                ListOfCategories = p.ListOfCategories?.Select(lc => new ListOfCategoriesResponseDto
                {
                    IdListOfCategories = lc.IdListOfCategories,
                    ProjectCategory = lc.ProjectCategory != null ? new ProjectCategoryResponseDto
                    {
                        IdProjectCategory = lc.ProjectCategory.IdProjectCategory,
                        Name = lc.ProjectCategory.Name ?? string.Empty,
                        NormalizedName = lc.ProjectCategory.NormalizedName
                    } : null
                }).ToList() ?? new List<ListOfCategoriesResponseDto>()
            }).ToList()
        })
        .OrderBy(c => c.IdCustomer)
        .ToList();

    return customerGroups;
}
```

---

## 6. Controlador (WebApi/Projects/Controllers/ProjectController.cs)

### Endpoint Agregado

```csharp
/// <summary>
/// Obtiene clientes con sus proyectos asociados (Endpoint principal GET)
/// </summary>
/// <remarks>
/// Este endpoint devuelve una lista de clientes, cada uno con sus proyectos asociados.
/// 
/// Filtros opcionales:
/// - IdCustomer: Filtrar por un cliente específico
/// - CurrentHito: Filtrar por el estado actual del proyecto (Appointment, Design, Production, etc.)
/// - CategoryIds: Filtrar proyectos que contengan al menos una de las categorías especificadas
/// 
/// La respuesta incluye:
/// - Información completa del cliente (nombre, email, teléfono, configuraciones)
/// - Lista de proyectos del cliente con:
///   * Datos del proyecto (dirección, estado actual, citas, visitas)
///   * Cotización del proyecto (totales e impuestos)
///   * Categorías asociadas al proyecto
/// </remarks>
/// <param name="idCustomer">ID del cliente (opcional)</param>
/// <param name="currentHito">Estado actual del proyecto (opcional)</param>
/// <param name="categoryIds">Lista de IDs de categorías para filtrar (opcional)</param>
/// <returns>Lista de clientes con sus proyectos</returns>
/// <response code="200">Lista de clientes con proyectos obtenida exitosamente</response>
/// <response code="500">Error interno del servidor</response>
[HttpGet]
[ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerWithProjectsResponseDto>>), 200)]
[ProducesResponseType(typeof(ApiResponse<object>), 500)]
public async Task<ActionResult> GetCustomersWithProjects(
    [FromQuery] int? idCustomer = null,
    [FromQuery] Hitos? currentHito = null,
    [FromQuery] List<int>? categoryIds = null)
{
    try
    {
        var filter = new GetProjectsWithCustomersFilterDto
        {
            IdCustomer = idCustomer,
            CurrentHito = currentHito,
            CategoryIds = categoryIds
        };

        var result = await _projectService.GetCustomersWithProjectsAsync(filter);

        return Ok(new ApiResponse<IEnumerable<CustomerWithProjectsResponseDto>>(
            result,
            "Customers with projects retrieved successfully",
            200
        ));
    }
    catch (Exception ex)
    {
        return StatusCode(500, new ApiResponse<object>(
            null,
            $"Internal server error: {ex.Message}",
            500
        ));
    }
}
```

---

## 7. Ejemplos de Uso del Endpoint

### Ruta Base
```
GET /api/projects/Project
```

### Ejemplos de Peticiones

#### 1. Obtener todos los clientes con sus proyectos
```http
GET /api/projects/Project
```

#### 2. Filtrar por un cliente específico
```http
GET /api/projects/Project?idCustomer=1
```

#### 3. Filtrar por estado del proyecto (Hito)
```http
GET /api/projects/Project?currentHito=Appointment
```

**Valores posibles de `CurrentHito`:**
- `Appointment` (0)
- `Design` (1)
- `Production` (2)
- `Installation` (3)
- `Completed` (4)
- `Cancelled` (5)

#### 4. Filtrar por categorías
```http
GET /api/projects/Project?categoryIds=1&categoryIds=2
```

#### 5. Combinación de filtros
```http
GET /api/projects/Project?idCustomer=1&currentHito=Design&categoryIds=1&categoryIds=2
```

---

## 8. Estructura de Respuesta JSON

### Ejemplo de Respuesta Exitosa

```json
{
  "data": [
    {
      "idCustomer": 1,
      "externalId": 321,
      "name": "Juan Carlos",
      "lastName": "Lopez Portillo",
      "email": "juan.perez@example.com",
      "phoneNumber": "+54 999 0012 123",
      "taxId": "LOPJ850101ABC",
      "settingsCustomerJson": {
        "index": 1,
        "route": "Foraneo",
        "type": "Aluminiero",
        "discount": 5.0
      },
      "projects": [
        {
          "idProject": 101,
          "currentHito": "Appointment",
          "writtenAddress": "Calle Ficticia 123, Col. Centro",
          "exactAddress": "Calle Ficticia 123, Col. Centro, CDMX, México",
          "appointmentJson": {
            "index": 1,
            "name": "Juan Carlos",
            "lastName": "Lopez Portillo",
            "phoneNumber": "+54 999 0012 123",
            "datetime": "2024-01-15T10:00:00Z",
            "notes": "Cliente interesado en ventanas de aluminio",
            "direction": "Calle Ficticia 123",
            "reference": "Casa azul con portón blanco"
          },
          "visitsJson": [
            {
              "index": 1,
              "dateVisite": "2024-01-15T10:30:00Z",
              "description": "Primera visita para toma de medidas",
              "idUser": 1,
              "type": "Medición",
              "createdAt": "2024-01-15T09:00:00Z"
            }
          ],
          "projectQuote": {
            "idProjectQuotes": 1001,
            "grandTotal": 15000.0,
            "totalTaxes": 2400.0
          },
          "listOfCategories": [
            {
              "idListOfCategories": 1,
              "projectCategory": {
                "idProjectCategory": 1,
                "name": "Ventanas",
                "normalizedName": "VENTANAS"
              }
            }
          ]
        },
        {
          "idProject": 102,
          "currentHito": "Design",
          "writtenAddress": "Avenida Principal 456",
          "exactAddress": "Avenida Principal 456, Col. Jardines, CDMX, México",
          "appointmentJson": null,
          "visitsJson": [],
          "projectQuote": {
            "idProjectQuotes": 1002,
            "grandTotal": 25000.0,
            "totalTaxes": 4000.0
          },
          "listOfCategories": [
            {
              "idListOfCategories": 2,
              "projectCategory": {
                "idProjectCategory": 2,
                "name": "Puertas",
                "normalizedName": "PUERTAS"
              }
            }
          ]
        }
      ]
    }
  ],
  "message": "Customers with projects retrieved successfully",
  "statusCode": 200
}
```

### Ejemplo de Respuesta de Error

```json
{
  "data": null,
  "message": "Internal server error: Connection to database failed",
  "statusCode": 500
}
```

---

## 9. Ventajas de esta Implementación

✅ **Separación de responsabilidades**: Controller → Service → Repository  
✅ **Consultas optimizadas**: Un solo viaje a la base de datos con Include/ThenInclude  
✅ **Filtros dinámicos**: Se aplican solo los filtros que vienen en la petición  
✅ **DTOs específicos**: No se exponen entidades directamente  
✅ **Async/Await**: Operaciones asíncronas para mejor rendimiento  
✅ **AsNoTracking**: Mejora el rendimiento en consultas de solo lectura  
✅ **Agrupación por cliente**: Estructura jerárquica clara  
✅ **Documentación Swagger**: XML comments completos para la API  

---

## 10. Posibles Mejoras Futuras

- [ ] Implementar paginación para grandes volúmenes de datos
- [ ] Agregar ordenamiento personalizado (por fecha, nombre, etc.)
- [ ] Implementar caché para consultas frecuentes
- [ ] Agregar más filtros (rango de fechas, búsqueda por texto, etc.)
- [ ] Implementar proyección selectiva de campos (GraphQL style)
- [ ] Agregar logging detallado
- [ ] Implementar validación de permisos/autorización

---

## 11. Estado de Compilación

✅ **Proyecto compilado exitosamente**  
✅ **Sin errores críticos**  
⚠️ **52 warnings** (relacionados con nullable reference types - normales en .NET 9)  

---

**Fecha de implementación**: 24 de diciembre de 2025  
**Versión**: 1.0  
**Desarrollador**: Sistema AVASphere

