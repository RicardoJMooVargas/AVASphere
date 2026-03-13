﻿namespace AVASphere.ApplicationCore.Common.DTOs;

using AVASphere.ApplicationCore.Common.Entities.General;

public class RolRequestDto
{
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }
    public int IdArea { get; set; }
    
    // ✅ AGREGAR: Propiedades para Permissions y Modules
    public List<Permission>? Permissions { get; set; }
    public List<Module>? Modules { get; set; }
}

public class RolResponseDto
{
    public int IdRol { get; set; }
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }
    public int IdArea { get; set; }
    public string AreaName { get; set; } = null!;
    public int UserCount { get; set; }
    
    // ✅ AGREGAR: Propiedades para mostrar Permissions y Modules en respuesta
    public List<Permission>? Permissions { get; set; }
    public List<Module>? Modules { get; set; }
}