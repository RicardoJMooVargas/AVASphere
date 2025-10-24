using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.System.Services
{
    public class DbToolsServices
    {
        private readonly MasterDbContext _dbContext;
        private readonly ILogger<DbToolsServices> _logger;
        private readonly IConfiguration _configuration;

        public DbToolsServices(MasterDbContext dbContext, ILogger<DbToolsServices> logger, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _configuration = configuration;
        }

        // 1️⃣ Verificar conexión y existencia de datos
        public async Task<(bool IsConnected, bool HasData, string Message)> CheckConnectionAsync()
        {
            try
            {
                // 1️⃣ Verificar conexión
                var canConnect = await _dbContext.Database.CanConnectAsync();
                if (!canConnect)
                    return (false, false, "No se pudo conectar a la base de datos.");

                // 2️⃣ Revisar si existen tablas (sin usar EF Query)
                int tableCount = 0;
                await using (var connection = _dbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    await using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM information_schema.tables 
                    WHERE table_schema = 'public';";

                        var result = await command.ExecuteScalarAsync();
                        tableCount = Convert.ToInt32(result);
                    }
                }

                var hasTables = tableCount > 0;
                return hasTables
                    ? (true, true, "Conexión OK. Existen tablas en la base de datos.")
                    : (true, false, "Conexión OK. No existen tablas (base vacía o sin migrar).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comprobando conexión con la base de datos");
                return (false, false, $"Error: {ex.Message}");
            }
        }

        // 2️⃣ Crear una migración (solo si DB vacía)
        public async Task<string> CreateMigrationAsync(string migrationName)
        {
            var (connected, hasData, msg) = await CheckConnectionAsync();
            if (!connected) return $"❌ {msg}";
            if (hasData) return "⚠️ La base contiene datos. No se puede crear una nueva migración.";

            try
            {
                // Obtener rutas desde la configuración inyectada
                var infraPath = _configuration["EfTools:InfrastructureProjectPath"];
                var startupPath = _configuration["EfTools:StartupProjectPath"];
                var migrationsFolder = _configuration["EfTools:MigrationsFolder"];

                // Si no hay configuración, usar valores por defecto basados en tu estructura
                if (string.IsNullOrEmpty(infraPath))
                {
                    infraPath = "../AVASphere.Infrastructure/AVASphere.Infrastructure.csproj";
                }

                if (string.IsNullOrEmpty(startupPath))
                {
                    startupPath = "../AVASphere.WebApi/AVASphere.WebApi.csproj";
                }

                if (string.IsNullOrEmpty(migrationsFolder))
                {
                    migrationsFolder = "Common/Migrations";
                }

                // Construir rutas absolutas
                var basePath = Directory.GetCurrentDirectory();
                var absoluteInfraPath = Path.GetFullPath(Path.Combine(basePath, infraPath));
                var absoluteStartupPath = Path.GetFullPath(Path.Combine(basePath, startupPath));

                _logger.LogInformation($"Buscando proyecto Infra en: {absoluteInfraPath}");
                _logger.LogInformation($"Buscando proyecto WebApi en: {absoluteStartupPath}");

                // Verificar que los archivos .csproj existen
                if (!File.Exists(absoluteInfraPath))
                {
                    // Intentar encontrar el proyecto automáticamente
                    absoluteInfraPath = FindProjectFile("AVASphere.Infrastructure.csproj");
                    if (string.IsNullOrEmpty(absoluteInfraPath))
                    {
                        return $"❌ No se encuentra el proyecto de infraestructura. Buscado en: {Path.Combine(basePath, infraPath)}";
                    }
                }

                if (!File.Exists(absoluteStartupPath))
                {
                    // Intentar encontrar el proyecto automáticamente
                    absoluteStartupPath = FindProjectFile("AVASphere.WebApi.csproj");
                    if (string.IsNullOrEmpty(absoluteStartupPath))
                    {
                        return $"❌ No se encuentra el proyecto WebApi. Buscado en: {Path.Combine(basePath, startupPath)}";
                    }
                }

                // Construir comando EF
                var command = $"dotnet ef migrations add {migrationName} " +
                             $"--project \"{absoluteInfraPath}\" " +
                             $"--startup-project \"{absoluteStartupPath}\" " +
                             $"--output-dir \"{migrationsFolder}\"";

                _logger.LogInformation($"Ejecutando comando EF: {command}");
                var result = await ExecuteEfCommandAsync(command);

                if (result.Contains("ERROR:") || result.Contains("error"))
                {
                    return $"❌ Error en EF: {result}";
                }

                return string.IsNullOrEmpty(result) ? "✅ Migración creada exitosamente." : $"✅ {result}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando migración");
                return $"❌ Error creando migración: {ex.Message}";
            }
        }

        // 3️⃣ Aplicar migraciones (solo si DB vacía)
        public async Task<string> ApplyMigrationAsync()
        {
            var (connected, hasData, msg) = await CheckConnectionAsync();
            if (!connected) return $"❌ {msg}";
            if (hasData) return "⚠️ La base contiene datos. No se puede aplicar migraciones.";

            try
            {
                await _dbContext.Database.MigrateAsync();
                return "✅ Migraciones aplicadas correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aplicando migraciones");
                return $"❌ Error aplicando migraciones: {ex.Message}";
            }
        }

        // 4️⃣ Eliminar la base de datos
        public async Task<string> DropDatabaseAsync()
        {
            try
            {
                var deleted = await _dbContext.Database.EnsureDeletedAsync();
                return deleted ? "🗑️ Base de datos eliminada correctamente." : "⚠️ No se eliminó ninguna base.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando la base de datos");
                return $"❌ Error eliminando base de datos: {ex.Message}";
            }
        }

        // Ejecutar comando EF CLI desde backend
        private static async Task<string> ExecuteEfCommandAsync(string command)
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            using var process = new Process();

            if (isWindows)
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {command}";
            }
            else
            {
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{command}\"";
            }

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var result = string.IsNullOrWhiteSpace(error) ? output : $"ERROR: {error}";
            
            // Si no hay output pero tampoco error, considerar éxito
            if (string.IsNullOrWhiteSpace(result) && process.ExitCode == 0)
            {
                return "Comando ejecutado exitosamente";
            }

            return result;
        }

        // Método auxiliar para encontrar archivos de proyecto
        private string FindProjectFile(string projectFileName)
        {
            try
            {
                var currentDir = Directory.GetCurrentDirectory();
                var files = Directory.GetFiles(currentDir, projectFileName, SearchOption.AllDirectories);
                
                if (files.Length > 0)
                {
                    return files[0];
                }

                // Buscar en directorio padre
                var parentDir = Directory.GetParent(currentDir);
                if (parentDir != null)
                {
                    files = Directory.GetFiles(parentDir.FullName, projectFileName, SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        return files[0];
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error buscando archivo {projectFileName}");
                return null;
            }
        }
    }
}