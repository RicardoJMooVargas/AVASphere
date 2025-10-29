using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.Infrastructure;

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
    // Crear documentación Swagger solo para los módulos funcionales
    foreach (var moduleValue in Enum.GetValues(typeof(SystemModule)))
    {
        var module = (SystemModule)moduleValue;
        var name = module.ToString().ToLower();

        c.SwaggerDoc(name, new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = $"VYAA Central Infor API - {module} SystemModule",
            Version = "v1",
            Description = GetModuleDescription(module),
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "VYAA Team"
            }
        });
    }

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

    // Asignar controladores a su documento Swagger
    c.DocInclusionPredicate((docName, apiDescription) =>
    {
        if (apiDescription.GroupName != null)
        {
            return docName == apiDescription.GroupName.ToLower();
        }

        // 🔹 Ignorar controladores sin grupo (no mostrar "general")
        return false;
    });

    // Configurar tags por controlador
    c.TagActionsBy(api =>
    {
        var controllerActionDescriptor = api.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        if (controllerActionDescriptor != null)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }

        return new[] { "Default" };
    });

    // Ordenar las acciones en Swagger
    c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");

    // Incluir comentarios XML si existen
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Función auxiliar para descripciones de módulos
static string GetModuleDescription(SystemModule module) => module switch
{
    SystemModule.Common => "Gestión de usuarios, autenticación y configuración del sistema",
    SystemModule.Sales => "Gestión de ventas y seguimiento de clientes",
    SystemModule.Projects => "Gestión y planificación de proyectos",
    SystemModule.Inventory => "Control de inventarios y artículos para venta",
    SystemModule.Shopping => "Gestión de compras y proveedores",
    SystemModule.System => "Funciones de mantenimiento del sistema y administración",
    _ => "Módulo no documentado",
};



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
        c.SwaggerEndpoint("/swagger/common/swagger.json", "Common SystemModule - User & Authentication"); // <- AGREGAR ESTA LÍNEA
        c.SwaggerEndpoint("/swagger/system/swagger.json", "System SystemModule - System Management");
        c.SwaggerEndpoint("/swagger/sales/swagger.json", "Sales SystemModule - Sales & Tracking");
        
        c.RoutePrefix = "swagger";
        c.DefaultModelsExpandDepth(-1);
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DisplayRequestDuration();
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

