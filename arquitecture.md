# VYAA Central Infor API - Documentación de Arquitectura

# 1. Requerimientos de Deploy

### 1.1 Base de Datos MongoDB

#### Instalación y Configuración de MongoDB
1. **Instalar MongoDB Community Server** desde https://www.mongodb.com/try/download/community
2. **Configurar MongoDB con autenticación**:
   ```bash
   # Iniciar MongoDB sin autenticación
   mongod --port 27017 --dbpath /data/db

   # Conectar al shell de MongoDB
   mongo

   # Crear usuario administrador
   use admin
   db.createUser({
     user: "admin",
     pwd: "admin123",
     roles: [ { role: "userAdminAnyDatabase", db: "admin" }, "readWriteAnyDatabase" ]
   })

   # Habilitar autenticación
   # Reiniciar MongoDB con: mongod --auth --port 27017 --dbpath /data/db
   ```

3. **Crear la base de datos del proyecto**:
   ```bash
   # Conectar con autenticación
   mongo -u admin -p admin123 --authenticationDatabase admin

   # Crear la base de datos
   use VYAACentralInforDB
   ```

### 1.2 Configuración de appsettings.json

#### appsettings.json (Producción)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDB": {
    "ConnectionString": "mongodb://admin:admin123@localhost:27017",
    "DatabaseName": "VYAACentralInforDB"
  }
}
```

#### appsettings.Development.json (Desarrollo)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "MongoDB": {
    "ConnectionString": "mongodb://admin:admin123@localhost:27017",
    "DatabaseName": "VYAACentralInforDB_Dev"
  }
}
```

### 1.3 Variables de Entorno (Alternativa Segura)
Para producción, se recomienda usar variables de entorno:
```bash
export MONGODB_CONNECTION_STRING="mongodb://username:password@host:port"
export MONGODB_DATABASE_NAME="VYAACentralInforDB"
```

# 2. Distribución de Carpetas y Responsabilidades

### 2.1 Estructura General del Proyecto
```
VYAACentralInforApi/
├── src/
│   ├── VYAACentralInforApi.Domain/        # Capa de Dominio
│   ├── VYAACentralInforApi.Application/   # Capa de Aplicación
│   ├── VYAACentralInforApi.Infrastructure/ # Capa de Infraestructura
│   └── VYAACentralInforApi.WebApi/        # Capa de Presentación
├── dockerfile.mongodb                      # Docker para MongoDB
├── readme.md                              # Documentación del proyecto
└── VYAACentralInforApi.sln               # Solución de Visual Studio
```

### 2.2 Capa de Dominio (Domain)
**Responsabilidad**: Contiene las entidades de negocio y las interfaces de repositorio.

```
VYAACentralInforApi.Domain/
├── Common/                    # Entidades y interfaces comunes
├── System/                    # Módulo del Sistema
│   ├── Entities/             # Entidades del dominio (Users, Roles, etc.)
│   └── Interfaces/           # Interfaces de repositorio (IUserRepository)
└── Sales/                     # Módulo de Ventas (futuro)
    ├── Entities/             # Entidades de ventas (Customer, Product, Order)
    └── Interfaces/           # Interfaces de repositorio de ventas
```

### 2.3 Capa de Aplicación (Application)
**Responsabilidad**: Contiene la lógica de negocio y servicios de aplicación.

```
VYAACentralInforApi.Application/
├── Common/                    # Servicios y DTOs comunes
├── System/                    # Módulo del Sistema
│   ├── Interfaces/           # Interfaces de servicios (IUserService)
│   └── Services/             # Implementación de servicios (UserService)
└── Sales/                     # Módulo de Ventas (futuro)
    ├── Interfaces/           # Interfaces de servicios de ventas
    └── Services/             # Servicios de ventas
```

### 2.4 Capa de Infraestructura (Infrastructure)
**Responsabilidad**: Implementa las interfaces de repositorio y maneja la persistencia de datos.

```
VYAACentralInforApi.Infrastructure/
├── System/                    # Módulo del Sistema
│   ├── Data/                 # Contexto de base de datos del sistema
│   │   └── SystemMongoDbContext.cs
│   └── Repositories/         # Implementación de repositorios
│       └── UserRepository.cs
├── Sales/                     # Módulo de Ventas (futuro)
│   ├── Data/                 # Contexto de ventas
│   └── Repositories/         # Repositorios de ventas
├── Common/                    # Infraestructura común
├──   DatabaseInitializationService.cs # Servicio para inicializar datos
└── DependencyInjection.cs    # Configuración de inyección de dependencias
```

### 2.5 Capa de Presentación (WebApi)
**Responsabilidad**: Expone la API REST y maneja las peticiones HTTP.

```
VYAACentralInforApi.WebApi/
├── System/                    # Controladores del módulo Sistema
├── Sales/                     # Controladores del módulo Ventas (futuro)
├── Common/                    # Controladores comunes
├── Properties/                # Configuración del proyecto
├── appsettings.json          # Configuración de producción
├── appsettings.Development.json # Configuración de desarrollo
└── Program.cs                # Punto de entrada de la aplicación
```

# <span style="color:deepskyblue">3. Guia para generar un controlador</span>

### 3.1 Proceso Completo de Creación

#### Paso 1: Crear la Entidad en Domain
```csharp
// Domain/[Módulo]/Entities/Product.cs
namespace VYAACentralInforApi.Domain.Sales.Entities
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? IdProduct { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateTime CreateAt { get; set; }
        public bool Status { get; set; } = true;
    }
}
```

#### Paso 2: Crear la Interfaz de Repositorio en Domain
```csharp
// Domain/[Módulo]/Interfaces/IProductRepository.cs
namespace VYAACentralInforApi.Domain.Sales.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(string id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(string id);
    }
}
```

#### Paso 3: Implementar el Repositorio en Infrastructure
```csharp
// Infrastructure/[Módulo]/Repositories/ProductRepository.cs
namespace VYAACentralInforApi.Infrastructure.Sales.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _productsCollection;

        public ProductRepository(SalesMongoDbContext context)
        {
            _productsCollection = context.Products;
        }

        // Implementar todos los métodos de la interfaz...
    }
}
```

#### Paso 4: Configurar el Contexto de Base de Datos
```csharp
// Infrastructure/[Módulo]/Data/SalesMongoDbContext.cs
namespace VYAACentralInforApi.Infrastructure.Sales.Data
{
    public class SalesMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public SalesMongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");
        // Agregar más colecciones según sea necesario...
    }
}
```

#### Paso 5: Crear la Interfaz de Servicio en Application
```csharp
// Application/[Módulo]/Interfaces/IProductService.cs
namespace VYAACentralInforApi.Application.Sales.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(string id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(string id);
    }
}
```

#### Paso 6: Implementar el Servicio en Application
```csharp
// Application/[Módulo]/Services/ProductService.cs
namespace VYAACentralInforApi.Application.Sales.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Implementar lógica de negocio y validaciones...
    }
}
```

#### Paso 7: Configurar en DependencyInjection.cs
```csharp
// Infrastructure/DependencyInjection.cs
private static void AddSalesServices(IServiceCollection services)
{
    // CONTEXTO DE BASE DE DATOS
    services.AddSingleton<SalesMongoDbContext>(sp => 
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetSection("MongoDB:ConnectionString").Value;
        var databaseName = configuration.GetSection("MongoDB:DatabaseName").Value;
        return new SalesMongoDbContext(connectionString!, databaseName!);
    });

    // REPOSITORIOS DEL MÓDULO SALES
    services.AddScoped<IProductRepository, ProductRepository>();

    // SERVICIOS DEL MÓDULO SALES
    services.AddScoped<IProductService, ProductService>();
}
```

#### Paso 8: Crear el Controlador en WebApi
```csharp
// WebApi/[Módulo]/ProductController.cs
namespace VYAACentralInforApi.WebApi.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            var createdProduct = await _productService.CreateProductAsync(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.IdProduct }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(string id, Product product)
        {
            product.IdProduct = id;
            var updatedProduct = await _productService.UpdateProductAsync(product);
            return Ok(updatedProduct);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(string id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}
```

### 3.2 Checklist de Implementación

#### ✅ **Antes de Crear el Controlador, Asegúrate de:**
1. **Entidad creada** en `Domain/[Módulo]/Entities/`
2. **Interfaz de repositorio** en `Domain/[Módulo]/Interfaces/`
3. **Repositorio implementado** en `Infrastructure/[Módulo]/Repositories/`
4. **Contexto de DB configurado** en `Infrastructure/[Módulo]/Data/`
5. **Interfaz de servicio** en `Application/[Módulo]/Interfaces/`
6. **Servicio implementado** en `Application/[Módulo]/Services/`
7. **Dependencias registradas** en `DependencyInjection.cs`

#### ✅ **Patrón de Nomenclatura:**
- **Entidades**: `Product`, `Customer`, `Order`
- **Interfaces**: `IProductRepository`, `IProductService`
- **Implementaciones**: `ProductRepository`, `ProductService`
- **Controladores**: `ProductController`

#### ✅ **Convenciones de Rutas:**
- **GET All**: `GET /api/product`
- **GET By ID**: `GET /api/product/{id}`
- **POST**: `POST /api/product`
- **PUT**: `PUT /api/product/{id}`
- **DELETE**: `DELETE /api/product/{id}`

### 3.3 Notas Importantes

- **Separación por Módulos**: Cada módulo (System, Sales, etc.) debe tener su propio contexto de base de datos
- **Inyección de Dependencias**: Todas las dependencias deben registrarse en `DependencyInjection.cs`
- **Clean Architecture**: Respetar las dependencias entre capas (Domain ← Application ← Infrastructure ← WebApi)
- **Inicialización**: Si necesitas datos iniciales, agrégalos al `DatabaseInitializationService`
