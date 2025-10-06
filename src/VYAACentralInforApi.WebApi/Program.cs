using VYAACentralInforApi.Infrastructure;

// Manual .env file loader
LoadEnvironmentVariables();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCorsPolicy", corsBuilder =>
    {
        corsBuilder
            .SetIsOriginAllowed(origin =>
            {
                // Permitir cualquier localhost/127.0.0.1 con cualquier puerto
                var uri = new Uri(origin);
                return uri.Host == "localhost" || uri.Host == "127.0.0.1" || uri.Host == "::1";
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    
    options.AddPolicy("ProductionCorsPolicy", corsBuilder =>
    {
        corsBuilder
            .WithOrigins("https://yourdomain.com") // Cambiar por tu dominio en producción
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Documento principal
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "VYAA Central Infor API - General",
        Version = "v1",
        Description = "API para el sistema central de información VYAA - Endpoints generales",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "VYAA Team"
        }
    });

    // Documento para el módulo System
    c.SwaggerDoc("system", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "VYAA Central Infor API - System Module",
        Version = "v1",
        Description = "API para el módulo System - Gestión de usuarios, autenticación y configuración del sistema",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "VYAA Team"
        }
    });

    // Documento para el módulo Sales (futuro)
    c.SwaggerDoc("sales", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "VYAA Central Infor API - Sales Module",
        Version = "v1",
        Description = "API para el módulo Sales - Gestión de ventas y tracking",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "VYAA Team"
        }
    });
    
    // Configuración de seguridad JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Ingrese el token JWT con el prefijo 'Bearer '. Ejemplo: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    
    // Configurar qué endpoints van en cada documento
    c.DocInclusionPredicate((docName, apiDescription) =>
    {
        if (apiDescription.GroupName != null)
        {
            return docName == apiDescription.GroupName.ToLower();
        }

        // Los endpoints sin grupo van al documento principal
        return docName == "v1";
    });

    // Configuración simplificada para tags
    c.TagActionsBy(api =>
    {
        var controllerActionDescriptor = api.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        if (controllerActionDescriptor != null)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }

        return new[] { "Default" };
    });

    c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Infrastructure services (MongoDB, Repositories, Services, and Initialization)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Configurar múltiples endpoints de Swagger para cada área/módulo
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "General - API Status & Health");
        c.SwaggerEndpoint("/swagger/system/swagger.json", "System Module - Users & Authentication");
        c.SwaggerEndpoint("/swagger/sales/swagger.json", "Sales Module - Sales & Tracking");
        
        c.RoutePrefix = "swagger"; // Swagger UI will be available at /swagger
        
        // Configuraciones adicionales de UI
        c.DefaultModelsExpandDepth(-1); // Ocultar modelos por defecto
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // No expandir por defecto
        c.DisplayRequestDuration(); // Mostrar duración de requests
    });
}

app.UseHttpsRedirection();

// Enable CORS - debe estar antes de Authentication y Authorization
app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentCorsPolicy" : "ProductionCorsPolicy");

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Root endpoint to confirm API is running
app.MapGet("/", () => new
{
    message = "VYAA Central Infor API está funcionando correctamente",
    status = "OK",
    version = "v1.0.0",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    endpoints = new
    {
        swagger = "/swagger",
        health = "/health"
    }
})
.WithName("ApiStatus")
.WithTags("Status")
.Produces<object>(200);

// Health check endpoint
app.MapGet("/health", () => new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    uptime = Environment.TickCount64,
    version = "v1.0.0"
})
.WithName("HealthCheck")
.WithTags("Health")
.Produces<object>(200);

// Map controllers
app.MapControllers();

app.Run();

// Method to manually load .env file
static void LoadEnvironmentVariables()
{
    var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (File.Exists(envPath))
    {
        var lines = File.ReadAllLines(envPath);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
