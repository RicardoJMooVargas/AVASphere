﻿using AVASphere.ApplicationCore.Projects.DTOs;
using AVASphere.ApplicationCore.Projects.Enum;

namespace AVASphere.ApplicationCore.Projects.Interfaces;

public interface IProjectService
{
    // Create
    Task<ProjectResponseDto> CreateProjectAsync(ProjectCreateRequestDto projectRequest);
    Task<ProjectResponseDto> CreateProjectSimpleAsync(ProjectCreateSimpleRequestDto projectRequest);
    
    // Read
    Task<ProjectResponseDto?> GetProjectByIdAsync(int idProject);
    Task<ProjectDetailResponseDto?> GetProjectDetailByIdAsync(int idProject);
    Task<IEnumerable<ProjectResponseDto>> GetAllProjectsAsync();
    Task<IEnumerable<ProjectResponseDto>> GetProjectsByCustomerIdAsync(int idCustomer);
    Task<IEnumerable<ProjectResponseDto>> GetProjectsByHitoAsync(Hitos hito);
    Task<IEnumerable<ProjectResponseDto>> GetProjectsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProjectResponseDto>> GetProjectsByFilterAsync(ProjectFilterDto filter);
    
    // Update
    Task<ProjectResponseDto> UpdateProjectAsync(ProjectUpdateRequestDto projectRequest);
    Task<ProjectResponseDto> UpdateProjectHitoAsync(int idProject, Hitos newHito);
    Task<ProjectResponseDto> AddVisitToProjectAsync(AddVisitRequestDto visitRequest);
    
    // Delete
    Task<bool> DeleteProjectAsync(int idProject);
    
    // Utility Methods
    Task<bool> ProjectExistsAsync(int idProject);
    Task<int> GetTotalProjectsCountAsync();
    Task<int> GetProjectsCountByCustomerAsync(int idCustomer);
    Task<string> GenerateProjectCodeAsync(int idProject);
    
    // Main GET endpoint - Get customers with their projects
    Task<IEnumerable<CustomerWithProjectsResponseDto>> GetCustomersWithProjectsAsync(
        GetProjectsWithCustomersFilterDto? filter = null);
}