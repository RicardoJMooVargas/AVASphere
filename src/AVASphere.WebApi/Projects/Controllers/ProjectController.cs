using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Projects.DTOs;
using AVASphere.ApplicationCore.Projects.Enum;
using AVASphere.ApplicationCore.Projects.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Projects.Controllers;

[ApiController]
[Route("api/projects/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Projects))]
[Tags("Projects - Project")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

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

    /// <summary>
    /// Crea un nuevo proyecto (versión simplificada)
    /// </summary>
    /// <remarks>
    /// Esta versión simplificada solo requiere:
    /// - IdConfigSys: Identificador del sistema de configuración
    /// - IdCustomer: Identificador del cliente
    /// - IdProjectCategories: Lista de IDs de categorías del proyecto (al menos una)
    /// 
    /// El proyecto se crea con valores por defecto:
    /// - IdProjectQuote: 0 (se asignará cuando se cree la cotización)
    /// - CurrentHito: Appointment (Estado inicial)
    /// - VisitsJson: Lista vacía
    /// </remarks>
    /// <param name="request">Datos del proyecto a crear</param>
    /// <returns>El proyecto creado con sus datos básicos</returns>
    /// <response code="201">Proyecto creado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Cliente, configuración o categoría no encontrada</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult> CreateProject([FromBody] ProjectCreateSimpleRequestDto request)
    {
        try
        {
            // Validaciones básicas
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>(
                    null,
                    "Request body cannot be null",
                    400
                ));
            }

            if (request.IdConfigSys <= 0)
            {
                return BadRequest(new ApiResponse<object>(
                    null,
                    "IdConfigSys must be greater than 0",
                    400
                ));
            }

            if (request.IdCustomer <= 0)
            {
                return BadRequest(new ApiResponse<object>(
                    null,
                    "IdCustomer must be greater than 0",
                    400
                ));
            }

            if (request.IdProjectCategories == null || !request.IdProjectCategories.Any())
            {
                return BadRequest(new ApiResponse<object>(
                    null,
                    "At least one project category is required",
                    400
                ));
            }

            // Validar que no haya IDs duplicados en las categorías
            if (request.IdProjectCategories.Distinct().Count() != request.IdProjectCategories.Count)
            {
                return BadRequest(new ApiResponse<object>(
                    null,
                    "Duplicate project category IDs are not allowed",
                    400
                ));
            }

            // Validar que todos los IDs de categorías sean mayores a 0
            if (request.IdProjectCategories.Any(id => id <= 0))
            {
                return BadRequest(new ApiResponse<object>(
                    null,
                    "All project category IDs must be greater than 0",
                    400
                ));
            }

            // Crear el proyecto
            var createdProject = await _projectService.CreateProjectSimpleAsync(request);

            return CreatedAtAction(
                nameof(CreateProject),
                new ApiResponse<ProjectResponseDto>(
                    createdProject,
                    "Project created successfully",
                    201
                )
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>(
                null,
                ex.Message,
                400
            ));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>(
                null,
                ex.Message,
                404
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
}