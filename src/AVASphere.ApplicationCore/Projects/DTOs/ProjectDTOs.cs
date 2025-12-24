﻿using AVASphere.ApplicationCore.Projects.Entities.General;
using AVASphere.ApplicationCore.Projects.Enum;

namespace AVASphere.ApplicationCore.Projects.DTOs;

// Request DTOs
public class ProjectCreateRequestDto
{
    public int IdProjectQuote { get; set; }
    public int IdCustomer { get; set; }
    public int IdConfigSys { get; set; }
    public Hitos CurrentHito { get; set; } = Hitos.Appointment;
    public string? WrittenAddress { get; set; }
    public string? ExactAddress { get; set; }
    public AppointmentJson? AppointmentJson { get; set; }
}

// Simplified DTO for initial project creation
public class ProjectCreateSimpleRequestDto
{
    public int IdConfigSys { get; set; }
    public int IdCustomer { get; set; }
    public List<int> IdProjectCategories { get; set; } = new List<int>();
}

public class ProjectUpdateRequestDto
{
    public int IdProject { get; set; }
    public int? IdProjectQuote { get; set; }
    public int? IdCustomer { get; set; }
    public int? IdConfigSys { get; set; }
    public Hitos? CurrentHito { get; set; }
    public string? WrittenAddress { get; set; }
    public string? ExactAddress { get; set; }
    public AppointmentJson? AppointmentJson { get; set; }
}

public class AddVisitRequestDto
{
    public int IdProject { get; set; }
    public DateTime DateVisite { get; set; }
    public string? Description { get; set; }
    public int? IdUser { get; set; }
    public string? Type { get; set; }
}

// Response DTOs
public class ProjectResponseDto
{
    public int IdProject { get; set; }
    public int IdProjectQuote { get; set; }
    public int IdConfigSys { get; set; }
    public int IdCustomer { get; set; }
    public Hitos CurrentHito { get; set; }
    public string? WrittenAddress { get; set; }
    public string? ExactAddress { get; set; }
    public AppointmentJson? AppointmentJson { get; set; }
    public ICollection<VisitsJson>? VisitsJson { get; set; }
}

public class ProjectDetailResponseDto
{
    public int IdProject { get; set; }
    public int IdProjectQuote { get; set; }
    public int IdConfigSys { get; set; }
    public int IdCustomer { get; set; }
    public Hitos CurrentHito { get; set; }
    public string? WrittenAddress { get; set; }
    public string? ExactAddress { get; set; }
    public AppointmentJson? AppointmentJson { get; set; }
    public ICollection<VisitsJson>? VisitsJson { get; set; }
    
    // Related Data
    public CustomerSummaryDto? Customer { get; set; }
    public ConfigSysSummaryDto? ConfigSys { get; set; }
    public ICollection<CategorySummaryDto>? Categories { get; set; }
}

// Summary DTOs for related entities
public class CustomerSummaryDto
{
    public int IdCustomer { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

public class ConfigSysSummaryDto
{
    public int IdConfigSys { get; set; }
    public string? Name { get; set; }
}

public class CategorySummaryDto
{
    public int IdCategorie { get; set; }
    public string? Name { get; set; }
}

// Filter DTOs
public class ProjectFilterDto
{
    public int? IdCustomer { get; set; }
    public Hitos? CurrentHito { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
}

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

