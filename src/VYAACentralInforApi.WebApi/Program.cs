using VYAACentralInforApi.Infrastructure;
using VYAACentralInforApi.WebApi.System.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "VYAA Central Infor API",
        Version = "v1",
        Description = "API para el sistema central de información VYAA",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "VYAA Team"
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Infrastructure services (MongoDB, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Register hosted services
builder.Services.AddHostedService<DatabaseInitializationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VYAA Central Infor API v1");
        c.RoutePrefix = "swagger"; // Swagger UI will be available at /swagger
    });
}

app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

app.Run();
