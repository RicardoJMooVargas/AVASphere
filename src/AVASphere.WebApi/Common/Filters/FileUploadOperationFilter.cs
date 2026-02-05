using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace AVASphere.WebApi.Common.Filters;

/// <summary>
/// Filtro para manejar file uploads en Swagger UI
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        try
        {
            var parameters = context.MethodInfo.GetParameters();
            var hasFileParameter = parameters.Any(p => 
                p.ParameterType == typeof(IFormFile) || 
                p.ParameterType == typeof(IFormFile[]) ||
                p.ParameterType == typeof(IEnumerable<IFormFile>));

            if (!hasFileParameter) return;

            // Limpiar parámetros existentes que puedan causar conflicto
            operation.Parameters?.Clear();

            // Crear propiedades del formulario
            var formProperties = new Dictionary<string, OpenApiSchema>();
            
            foreach (var parameter in parameters)
            {
                if (parameter.ParameterType == typeof(IFormFile))
                {
                    formProperties[parameter.Name!] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = GetParameterDescription(parameter)
                    };
                }
                else if (parameter.ParameterType == typeof(bool))
                {
                    formProperties[parameter.Name!] = new OpenApiSchema
                    {
                        Type = "boolean",
                        Description = GetParameterDescription(parameter),
                        Default = parameter.HasDefaultValue ? 
                            new Microsoft.OpenApi.Any.OpenApiBoolean((bool)parameter.DefaultValue!) :
                            new Microsoft.OpenApi.Any.OpenApiBoolean(false)
                    };
                }
                else if (parameter.ParameterType == typeof(string))
                {
                    formProperties[parameter.Name!] = new OpenApiSchema
                    {
                        Type = "string",
                        Description = GetParameterDescription(parameter)
                    };
                }
            }

            // Configurar como multipart/form-data
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = formProperties,
                            Required = formProperties
                                .Where(p => !p.Key.Equals("overwrite", StringComparison.OrdinalIgnoreCase))
                                .Select(p => p.Key)
                                .ToHashSet()
                        }
                    }
                }
            };
        }
        catch (Exception ex)
        {
            // Log del error si es necesario, pero no romper Swagger
            Console.WriteLine($"Error en FileUploadOperationFilter: {ex.Message}");
        }
    }

    private Dictionary<string, OpenApiSchema> GetFormProperties(MethodInfo methodInfo)
    {
        var properties = new Dictionary<string, OpenApiSchema>();

        foreach (var parameter in methodInfo.GetParameters())
        {
            // Incluir todos los parámetros que tienen [FromForm] o son IFormFile
            var hasFromFormAttribute = parameter.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>() != null;
            var isFileParameter = parameter.ParameterType == typeof(IFormFile);
            
            if (!hasFromFormAttribute && !isFileParameter) continue;

            if (parameter.ParameterType == typeof(IFormFile))
            {
                properties[parameter.Name!] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = GetParameterDescription(parameter),
                    Nullable = false
                };
            }
            else if (parameter.ParameterType == typeof(bool))
            {
                var defaultValue = parameter.HasDefaultValue ? 
                    new Microsoft.OpenApi.Any.OpenApiBoolean((bool)parameter.DefaultValue!) :
                    new Microsoft.OpenApi.Any.OpenApiBoolean(false);
                    
                properties[parameter.Name!] = new OpenApiSchema
                {
                    Type = "boolean",
                    Description = GetParameterDescription(parameter),
                    Default = defaultValue,
                    Nullable = parameter.HasDefaultValue
                };
            }
            else if (parameter.ParameterType == typeof(string))
            {
                properties[parameter.Name!] = new OpenApiSchema
                {
                    Type = "string",
                    Description = GetParameterDescription(parameter),
                    Nullable = parameter.HasDefaultValue,
                    Default = parameter.HasDefaultValue && parameter.DefaultValue != null ? 
                        new Microsoft.OpenApi.Any.OpenApiString(parameter.DefaultValue.ToString()) : null
                };
            }
            else if (parameter.ParameterType == typeof(int))
            {
                properties[parameter.Name!] = new OpenApiSchema
                {
                    Type = "integer",
                    Description = GetParameterDescription(parameter),
                    Nullable = parameter.HasDefaultValue,
                    Default = parameter.HasDefaultValue ? 
                        new Microsoft.OpenApi.Any.OpenApiInteger((int)parameter.DefaultValue!) : null
                };
            }
        }

        return properties;
    }

    private string GetParameterDescription(ParameterInfo parameter)
    {
        return parameter.Name switch
        {
            "sqlFile" => "Archivo SQL a importar",
            "overwrite" => "Si debe sobrescribir datos existentes",
            _ => parameter.Name ?? "Parámetro"
        };
    }
}
