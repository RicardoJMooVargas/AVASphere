﻿using AVASphere.ApplicationCore.Projects.DTOs;
using AVASphere.ApplicationCore.Projects.Entities.General;
using AVASphere.ApplicationCore.Projects.Enum;
using AVASphere.ApplicationCore.Projects.Interfaces;
using AVASphere.Infrastructure.Projects.Repository;

namespace AVASphere.Infrastructure.Projects.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IProjectCategoryRepository _categoryRepository;
    private readonly ListOfCategoriesRepository _listOfCategoriesRepository;

    public ProjectService(
        IProjectRepository repository, 
        IProjectCategoryRepository categoryRepository,
        ListOfCategoriesRepository listOfCategoriesRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _listOfCategoriesRepository = listOfCategoriesRepository;
    }

    // Create
    public async Task<ProjectResponseDto> CreateProjectAsync(ProjectCreateRequestDto projectRequest)
    {
        if (projectRequest == null)
            throw new ArgumentNullException(nameof(projectRequest));

        var project = new Project
        {
            IdProjectQuote = projectRequest.IdProjectQuote,
            IdCustomer = projectRequest.IdCustomer,
            IdConfigSys = projectRequest.IdConfigSys,
            CurrentHito = projectRequest.CurrentHito,
            WrittenAddress = projectRequest.WrittenAddress,
            ExactAddress = projectRequest.ExactAddress,
            AppointmentJson = projectRequest.AppointmentJson,
            VisitsJson = new List<VisitsJson>()
        };

        var created = await _repository.CreateProjectAsync(project);
        return MapToDto(created);
    }

    /// <summary>
    /// Crea un proyecto de forma simplificada (versión inicial)
    /// Solo requiere IdConfigSys, IdCustomer y lista de categorías
    /// </summary>
    public async Task<ProjectResponseDto> CreateProjectSimpleAsync(ProjectCreateSimpleRequestDto projectRequest)
    {
        if (projectRequest == null)
            throw new ArgumentNullException(nameof(projectRequest));

        // Validar que las categorías existan (se hace en ListOfCategoriesRepository)
        if (projectRequest.IdProjectCategories == null || !projectRequest.IdProjectCategories.Any())
            throw new ArgumentException("At least one project category is required", nameof(projectRequest.IdProjectCategories));

        // Crear proyecto con valores por defecto
        var project = new Project
        {
            IdProjectQuote = 0, // Se asignará posteriormente cuando se cree la cotización
            IdCustomer = projectRequest.IdCustomer,
            IdConfigSys = projectRequest.IdConfigSys,
            CurrentHito = Hitos.Appointment, // Estado inicial
            VisitsJson = new List<VisitsJson>()
        };

        // Guardar proyecto
        var created = await _repository.CreateProjectAsync(project);

        // Crear las categorías asociadas al proyecto
        await _listOfCategoriesRepository.CreateCategoriesForProjectAsync(
            created.IdProject, 
            projectRequest.IdProjectCategories);

        return MapToDto(created);
    }

    // Read
    public async Task<ProjectResponseDto?> GetProjectByIdAsync(int idProject)
    {
        var project = await _repository.GetProjectByIdAsync(idProject);
        return project != null ? MapToDto(project) : null;
    }

    public async Task<ProjectDetailResponseDto?> GetProjectDetailByIdAsync(int idProject)
    {
        var project = await _repository.GetProjectWithRelationsAsync(idProject);
        return project != null ? MapToDetailDto(project) : null;
    }

    public async Task<IEnumerable<ProjectResponseDto>> GetAllProjectsAsync()
    {
        var projects = await _repository.GetAllProjectsAsync();
        return projects.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectResponseDto>> GetProjectsByCustomerIdAsync(int idCustomer)
    {
        var projects = await _repository.GetProjectsByCustomerIdAsync(idCustomer);
        return projects.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectResponseDto>> GetProjectsByHitoAsync(Hitos hito)
    {
        var projects = await _repository.GetProjectsByHitoAsync((int)hito);
        return projects.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectResponseDto>> GetProjectsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var projects = await _repository.GetProjectsByDateRangeAsync(startDate, endDate);
        return projects.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectResponseDto>> GetProjectsByFilterAsync(ProjectFilterDto filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var allProjects = await _repository.GetAllProjectsAsync();
        var filtered = allProjects.AsQueryable();

        if (filter.IdCustomer.HasValue)
            filtered = filtered.Where(p => p.IdCustomer == filter.IdCustomer.Value);

        if (filter.CurrentHito.HasValue)
            filtered = filtered.Where(p => p.CurrentHito == filter.CurrentHito.Value);

        if (filter.StartDate.HasValue && filter.EndDate.HasValue)
        {
            filtered = filtered.Where(p => p.AppointmentJson != null &&
                                          p.AppointmentJson.Datetime >= filter.StartDate.Value &&
                                          p.AppointmentJson.Datetime <= filter.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            filtered = filtered.Where(p =>
                (p.WrittenAddress != null && p.WrittenAddress.ToLower().Contains(searchLower)) ||
                (p.ExactAddress != null && p.ExactAddress.ToLower().Contains(searchLower)));
        }

        return filtered.Select(MapToDto);
    }

    // Update
    public async Task<ProjectResponseDto> UpdateProjectAsync(ProjectUpdateRequestDto projectRequest)
    {
        if (projectRequest == null)
            throw new ArgumentNullException(nameof(projectRequest));

        var existingProject = await _repository.GetProjectByIdAsync(projectRequest.IdProject);
        if (existingProject == null)
            throw new KeyNotFoundException($"Project with Id {projectRequest.IdProject} not found.");

        // Selective update
        if (projectRequest.IdProjectQuote.HasValue)
            existingProject.IdProjectQuote = projectRequest.IdProjectQuote.Value;

        if (projectRequest.IdCustomer.HasValue)
            existingProject.IdCustomer = projectRequest.IdCustomer.Value;

        if (projectRequest.IdConfigSys.HasValue)
            existingProject.IdConfigSys = projectRequest.IdConfigSys.Value;

        if (projectRequest.CurrentHito.HasValue)
            existingProject.CurrentHito = projectRequest.CurrentHito.Value;

        if (projectRequest.WrittenAddress != null)
            existingProject.WrittenAddress = projectRequest.WrittenAddress;

        if (projectRequest.ExactAddress != null)
            existingProject.ExactAddress = projectRequest.ExactAddress;

        if (projectRequest.AppointmentJson != null)
            existingProject.AppointmentJson = projectRequest.AppointmentJson;

        var updated = await _repository.UpdateProjectAsync(existingProject);
        return MapToDto(updated);
    }

    public async Task<ProjectResponseDto> UpdateProjectHitoAsync(int idProject, Hitos newHito)
    {
        var existingProject = await _repository.GetProjectByIdAsync(idProject);
        if (existingProject == null)
            throw new KeyNotFoundException($"Project with Id {idProject} not found.");

        existingProject.CurrentHito = newHito;
        var updated = await _repository.UpdateProjectAsync(existingProject);
        return MapToDto(updated);
    }

    public async Task<ProjectResponseDto> AddVisitToProjectAsync(AddVisitRequestDto visitRequest)
    {
        if (visitRequest == null)
            throw new ArgumentNullException(nameof(visitRequest));

        var existingProject = await _repository.GetProjectByIdAsync(visitRequest.IdProject);
        if (existingProject == null)
            throw new KeyNotFoundException($"Project with Id {visitRequest.IdProject} not found.");

        // Initialize if null
        if (existingProject.VisitsJson == null)
            existingProject.VisitsJson = new List<VisitsJson>();

        // Calculate next index
        var nextIndex = existingProject.VisitsJson.Any()
            ? existingProject.VisitsJson.Max(v => v.Index) + 1
            : 1;

        var newVisit = new VisitsJson
        {
            Index = nextIndex,
            DateVisite = visitRequest.DateVisite,
            Description = visitRequest.Description,
            IdUser = visitRequest.IdUser,
            Type = visitRequest.Type,
            CreatedAt = DateTime.UtcNow
        };

        existingProject.VisitsJson.Add(newVisit);
        var updated = await _repository.UpdateProjectAsync(existingProject);
        return MapToDto(updated);
    }

    // Delete
    public async Task<bool> DeleteProjectAsync(int idProject)
    {
        return await _repository.DeleteProjectAsync(idProject);
    }

    // Utility Methods
    public async Task<bool> ProjectExistsAsync(int idProject)
    {
        return await _repository.ProjectExistsAsync(idProject);
    }

    public async Task<int> GetTotalProjectsCountAsync()
    {
        return await _repository.GetTotalProjectsCountAsync();
    }

    public async Task<int> GetProjectsCountByCustomerAsync(int idCustomer)
    {
        return await _repository.GetProjectsCountByCustomerAsync(idCustomer);
    }

    public async Task<string> GenerateProjectCodeAsync(int idProject)
    {
        var project = await _repository.GetProjectWithRelationsAsync(idProject);
        if (project == null)
            throw new KeyNotFoundException($"Project with Id {idProject} not found.");

        // Generar código: Iniciales del cliente + mes + año + categorias (hasta 4) + id de 6 dígitos
        // EJEMPLO: JD-0924-0001-000123

        var customerInitials = GetCustomerInitials(project.Customer);
        var monthYear = GetMonthYearFromAppointment(project);
        var categoryIds = GetCategoryIdsString(project);
        var projectIdPadded = idProject.ToString("D6");

        return $"{customerInitials}-{monthYear}-{categoryIds}-{projectIdPadded}";
    }

    // Private Helper Methods
    private static string GetCustomerInitials(AVASphere.ApplicationCore.Common.Entities.General.Customer? customer)
    {
        if (customer == null)
            return "XX";

        var nameInitial = !string.IsNullOrWhiteSpace(customer.Name)
            ? customer.Name[0].ToString().ToUpper()
            : "X";

        var lastNameInitial = !string.IsNullOrWhiteSpace(customer.LastName)
            ? customer.LastName[0].ToString().ToUpper()
            : "X";

        return $"{nameInitial}{lastNameInitial}";
    }

    private static string GetMonthYearFromAppointment(Project project)
    {
        var date = project.AppointmentJson?.Datetime ?? DateTime.UtcNow;
        return date.ToString("MMyy");
    }

    private static string GetCategoryIdsString(Project project)
    {
        if (project.ListOfCategories == null || !project.ListOfCategories.Any())
            return "0000";

        var categoryIds = project.ListOfCategories
            .Select(lc => lc.IdProjectCategory)
            .Take(4)
            .ToList();

        // Pad with zeros if less than 4 categories
        while (categoryIds.Count < 4)
            categoryIds.Add(0);

        return string.Join("", categoryIds.Select(id => id.ToString("D1")));
    }

    private static ProjectResponseDto MapToDto(Project project)
    {
        return new ProjectResponseDto
        {
            IdProject = project.IdProject,
            IdProjectQuote = project.IdProjectQuote,
            IdConfigSys = project.IdConfigSys,
            IdCustomer = project.IdCustomer,
            CurrentHito = project.CurrentHito,
            WrittenAddress = project.WrittenAddress,
            ExactAddress = project.ExactAddress,
            AppointmentJson = project.AppointmentJson,
            VisitsJson = project.VisitsJson
        };
    }

    private static ProjectDetailResponseDto MapToDetailDto(Project project)
    {
        return new ProjectDetailResponseDto
        {
            IdProject = project.IdProject,
            IdProjectQuote = project.IdProjectQuote,
            IdConfigSys = project.IdConfigSys,
            IdCustomer = project.IdCustomer,
            CurrentHito = project.CurrentHito,
            WrittenAddress = project.WrittenAddress,
            ExactAddress = project.ExactAddress,
            AppointmentJson = project.AppointmentJson,
            VisitsJson = project.VisitsJson,
            Customer = project.Customer != null ? new CustomerSummaryDto
            {
                IdCustomer = project.Customer.IdCustomer,
                Name = project.Customer.Name,
                LastName = project.Customer.LastName,
                PhoneNumber = project.Customer.PhoneNumber,
                Email = project.Customer.Email
            } : null,
            ConfigSys = project.ConfigSys != null ? new ConfigSysSummaryDto
            {
                IdConfigSys = project.ConfigSys.IdConfigSys,
                Name = project.ConfigSys.CompanyName
            } : null,
            Categories = project.ListOfCategories?
                .Where(lc => lc.ProjectCategory != null)
                .Select(lc => new CategorySummaryDto
                {
                    IdCategorie = lc.ProjectCategory.IdProjectCategory,
                    Name = lc.ProjectCategory.Name
                }).ToList()
        };
    }

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
}