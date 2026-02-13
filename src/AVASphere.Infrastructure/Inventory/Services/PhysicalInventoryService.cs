using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Enums;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.Products;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.Inventory.Services;

public class PhysicalInventoryService : IPhysicalInventoryService
{
    private readonly IPhysicalInventoryRepository _physicalInventoryRepository;
    private readonly IPhysicalInventoryDetailRepository _physicalInventoryDetailRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PhysicalInventoryService> _logger;

    public PhysicalInventoryService(
        IPhysicalInventoryRepository physicalInventoryRepository,
        IPhysicalInventoryDetailRepository physicalInventoryDetailRepository,
        IWarehouseRepository warehouseRepository,
        IInventoryRepository inventoryRepository,
        IUserRepository userRepository,
        ILogger<PhysicalInventoryService> logger)
    {
        _physicalInventoryRepository = physicalInventoryRepository;
        _physicalInventoryDetailRepository = physicalInventoryDetailRepository;
        _warehouseRepository = warehouseRepository;
        _inventoryRepository = inventoryRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<PhysicalInventoryResponseDto>> CreatePhysicalInventoryAsync(CreatePhysicalInventoryDto createDto, int userId)
    {
        try
        {
            _logger.LogInformation("Creating new PhysicalInventory for warehouse {WarehouseId} by user {UserId}", 
                createDto.IdWarehouse, userId);

            // Validar que el warehouse existe
            var warehouse = await _warehouseRepository.GetByIdAsync(createDto.IdWarehouse);
            if (warehouse == null)
            {
                return new ApiResponse<PhysicalInventoryResponseDto>
                {
                    Success = false,
                    Message = "El warehouse especificado no existe",
                    StatusCode = 400
                };
            }

            // Validar que el usuario existe
            var user = await _userRepository.SelectByIdAsync(userId);
            if (user == null)
            {
                return new ApiResponse<PhysicalInventoryResponseDto>
                {
                    Success = false,
                    Message = "El usuario especificado no existe",
                    StatusCode = 400
                };
            }

            // Crear la entidad PhysicalInventory
            var physicalInventory = new PhysicalInventory
            {
                InventoryDate = createDto.InventoryDate,
                Status = PhysicalInventoryStatus.Open.ToString(), // Status automáticamente como "Open"
                CreatedBy = userId, // ID obtenido del token
                Observations = createDto.Observations,
                IdWarehouse = createDto.IdWarehouse
            };

            // Crear en la base de datos
            var createdInventory = await _physicalInventoryRepository.CreateAsync(physicalInventory);

            // Mapear a DTO de respuesta
            var responseDto = await MapToPhysicalInventoryResponseDto(createdInventory);

            _logger.LogInformation("PhysicalInventory created successfully with ID {PhysicalInventoryId}", 
                createdInventory.IdPhysicalInventory);

            return new ApiResponse<PhysicalInventoryResponseDto>
            {
                Success = true,
                Data = responseDto,
                Message = "Conteo físico creado exitosamente",
                StatusCode = 201
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PhysicalInventory");
            return new ApiResponse<PhysicalInventoryResponseDto>
            {
                Success = false,
                Message = $"Error al crear el conteo físico: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<ApiResponse<PhysicalInventoryResponseDto>> UpdatePhysicalInventoryAsync(UpdatePhysicalInventoryDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating PhysicalInventory {PhysicalInventoryId}", updateDto.IdPhysicalInventory);

            // Verificar que el conteo físico existe
            var existingInventory = await _physicalInventoryRepository.GetByIdAsync(updateDto.IdPhysicalInventory);
            if (existingInventory == null)
            {
                return new ApiResponse<PhysicalInventoryResponseDto>
                {
                    Success = false,
                    Message = "El conteo físico especificado no existe",
                    StatusCode = 404
                };
            }

            // Verificar si se está intentando cambiar el IdWarehouse
            if (existingInventory.IdWarehouse != updateDto.IdWarehouse)
            {
                // Verificar si existen registros en PhysicalInventoryDetail
                var details = await _physicalInventoryDetailRepository.GetByPhysicalInventoryIdAsync(updateDto.IdPhysicalInventory);
                if (details.Any())
                {
                    return new ApiResponse<PhysicalInventoryResponseDto>
                    {
                        Success = false,
                        Message = "No se puede modificar el warehouse porque el conteo ya tiene registros de detalles asociados",
                        StatusCode = 400
                    };
                }

                // Validar que el nuevo warehouse existe
                var newWarehouse = await _warehouseRepository.GetByIdAsync(updateDto.IdWarehouse);
                if (newWarehouse == null)
                {
                    return new ApiResponse<PhysicalInventoryResponseDto>
                    {
                        Success = false,
                        Message = "El nuevo warehouse especificado no existe",
                        StatusCode = 400
                    };
                }
            }

            // Validar que el usuario existe
            var user = await _userRepository.SelectByIdAsync(updateDto.CreatedBy);
            if (user == null)
            {
                return new ApiResponse<PhysicalInventoryResponseDto>
                {
                    Success = false,
                    Message = "El usuario especificado no existe",
                    StatusCode = 400
                };
            }

            // Actualizar la entidad
            existingInventory.InventoryDate = updateDto.InventoryDate;
            existingInventory.Status = updateDto.Status;
            existingInventory.CreatedBy = updateDto.CreatedBy;
            existingInventory.Observations = updateDto.Observations;
            existingInventory.IdWarehouse = updateDto.IdWarehouse;

            // Actualizar en la base de datos
            var updatedInventory = await _physicalInventoryRepository.UpdateAsync(existingInventory);

            // Mapear a DTO de respuesta
            var responseDto = await MapToPhysicalInventoryResponseDto(updatedInventory);

            _logger.LogInformation("PhysicalInventory {PhysicalInventoryId} updated successfully", 
                updateDto.IdPhysicalInventory);

            return new ApiResponse<PhysicalInventoryResponseDto>
            {
                Success = true,
                Data = responseDto,
                Message = "Conteo físico actualizado exitosamente",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PhysicalInventory {PhysicalInventoryId}", updateDto.IdPhysicalInventory);
            return new ApiResponse<PhysicalInventoryResponseDto>
            {
                Success = false,
                Message = $"Error al actualizar el conteo físico: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<ApiResponse<bool>> DeletePhysicalInventoryAsync(int idPhysicalInventory, bool forceDelete = false)
    {
        try
        {
            _logger.LogInformation("Deleting PhysicalInventory {PhysicalInventoryId}, forceDelete: {ForceDelete}", 
                idPhysicalInventory, forceDelete);

            // Verificar que el conteo físico existe
            var physicalInventory = await _physicalInventoryRepository.GetByIdAsync(idPhysicalInventory);
            if (physicalInventory == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "El conteo físico especificado no existe",
                    StatusCode = 404
                };
            }

            // Verificar si existen registros en PhysicalInventoryDetail
            var details = await _physicalInventoryDetailRepository.GetByPhysicalInventoryIdAsync(idPhysicalInventory);
            var detailsList = details.ToList(); // Materializar para evitar enumeración múltiple
            var hasDetails = detailsList.Any();

            if (hasDetails && !forceDelete)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "No se puede eliminar el conteo físico porque tiene registros de detalles asociados. Use forceDelete=true para eliminación en cascada",
                    StatusCode = 400
                };
            }

            // Si tiene detalles y forceDelete es true, eliminar primero los detalles
            if (hasDetails && forceDelete)
            {
                _logger.LogInformation("Force deleting {DetailsCount} PhysicalInventoryDetails for PhysicalInventory {PhysicalInventoryId}", 
                    detailsList.Count, idPhysicalInventory);

                var deleteDetailsResult = await _physicalInventoryDetailRepository.DeleteByPhysicalInventoryIdAsync(idPhysicalInventory);
                if (!deleteDetailsResult)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Error al eliminar los detalles del conteo físico",
                        StatusCode = 500
                    };
                }
            }

            // Eliminar el conteo físico
            var deleteResult = await _physicalInventoryRepository.DeleteAsync(idPhysicalInventory);
            if (!deleteResult)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error al eliminar el conteo físico",
                    StatusCode = 500
                };
            }

            _logger.LogInformation("PhysicalInventory {PhysicalInventoryId} deleted successfully", idPhysicalInventory);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = hasDetails ? "Conteo físico y sus detalles eliminados exitosamente" : "Conteo físico eliminado exitosamente",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting PhysicalInventory {PhysicalInventoryId}", idPhysicalInventory);
            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = $"Error al eliminar el conteo físico: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<ApiResponse<PhysicalInventoryWithProductsDto>> GetPhysicalInventoryWithProductsAsync(int idPhysicalInventory, int userId)
    {
        try
        {
            _logger.LogInformation("Getting PhysicalInventory {PhysicalInventoryId} with products for user {UserId}", 
                idPhysicalInventory, userId);

            // Obtener el conteo físico
            var physicalInventory = await _physicalInventoryRepository.GetByIdWithDetailsAsync(idPhysicalInventory);
            if (physicalInventory == null)
            {
                return new ApiResponse<PhysicalInventoryWithProductsDto>
                {
                    Success = false,
                    Message = "El conteo físico especificado no existe",
                    StatusCode = 404
                };
            }

            // Obtener información del usuario para conocer su área
            var user = await _userRepository.SelectByIdAsync(userId);
            if (user == null)
            {
                return new ApiResponse<PhysicalInventoryWithProductsDto>
                {
                    Success = false,
                    Message = "El usuario especificado no existe",
                    StatusCode = 404
                };
            }

            // Obtener productos relacionados al warehouse
            var inventoryItems = await _inventoryRepository.GetByWarehouseIdAsync(physicalInventory.IdWarehouse);

            // Mapear los productos
            var products = inventoryItems.Select(inv => new ProductForInventoryDto
            {
                IdProduct = inv.Product.IdProduct,
                MainName = inv.Product.MainName,
                Description = inv.Product.Description,
                Unit = inv.Product.Unit,
                SupplierName = inv.Product.Supplier?.Name ?? "Sin proveedor",
                CurrentStock = inv.Stock,
                Location = inv.LocationDetail.HasValue ? new LocationInfoDto
                {
                    // Nota: Necesitaríamos acceso a LocationDetails para completar esta información
                    // Por ahora dejamos valores por defecto
                    IdLocationDetails = 0,
                    TypeStorageSystem = "N/A",
                    Section = "N/A",
                    VerticalLevel = 0,
                    AreaName = user.Rol?.Area?.Name ?? "Sin área",
                    StorageStructureCode = "N/A"
                } : null
            }).ToList();

            // Crear el DTO de respuesta
            var responseDto = new PhysicalInventoryWithProductsDto
            {
                IdPhysicalInventory = physicalInventory.IdPhysicalInventory,
                InventoryDate = physicalInventory.InventoryDate,
                Status = physicalInventory.Status,
                CreatedBy = physicalInventory.CreatedBy,
                CreatedByUserName = user.Name,
                Observations = physicalInventory.Observations,
                Warehouse = new WarehouseInfoDto
                {
                    IdWarehouse = physicalInventory.Warehouse.IdWarehouse,
                    Name = physicalInventory.Warehouse.Name,
                    Code = physicalInventory.Warehouse.Code,
                    Location = physicalInventory.Warehouse.Location
                },
                Products = products,
                UserArea = new UserAreaInfoDto
                {
                    IdArea = user.Rol?.Area?.IdArea ?? 0,
                    AreaName = user.Rol?.Area?.Name ?? "Sin área",
                    AreaNormalizedName = user.Rol?.Area?.NormalizedName ?? "Sin área"
                }
            };

            _logger.LogInformation("Retrieved PhysicalInventory {PhysicalInventoryId} with {ProductsCount} products", 
                idPhysicalInventory, products.Count);

            return new ApiResponse<PhysicalInventoryWithProductsDto>
            {
                Success = true,
                Data = responseDto,
                Message = "Conteo físico con productos obtenido exitosamente",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PhysicalInventory {PhysicalInventoryId} with products", idPhysicalInventory);
            return new ApiResponse<PhysicalInventoryWithProductsDto>
            {
                Success = false,
                Message = $"Error al obtener el conteo físico con productos: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<ApiResponse<PhysicalInventoryResponseDto>> GetPhysicalInventoryByIdAsync(int idPhysicalInventory)
    {
        try
        {
            var physicalInventory = await _physicalInventoryRepository.GetByIdAsync(idPhysicalInventory);
            if (physicalInventory == null)
            {
                return new ApiResponse<PhysicalInventoryResponseDto>
                {
                    Success = false,
                    Message = "El conteo físico especificado no existe",
                    StatusCode = 404
                };
            }

            var responseDto = await MapToPhysicalInventoryResponseDto(physicalInventory);

            return new ApiResponse<PhysicalInventoryResponseDto>
            {
                Success = true,
                Data = responseDto,
                Message = "Conteo físico obtenido exitosamente",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PhysicalInventory {PhysicalInventoryId}", idPhysicalInventory);
            return new ApiResponse<PhysicalInventoryResponseDto>
            {
                Success = false,
                Message = $"Error al obtener el conteo físico: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<PhysicalInventoryResponseDto>>> GetPhysicalInventoriesAsync(
        int? idWarehouse = null, 
        string? status = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        try
        {
            var physicalInventories = await _physicalInventoryRepository.GetFilteredAsync(
                null, startDate, endDate, status, null, null, idWarehouse);

            var responseDtos = new List<PhysicalInventoryResponseDto>();
            foreach (var inventory in physicalInventories)
            {
                var dto = await MapToPhysicalInventoryResponseDto(inventory);
                responseDtos.Add(dto);
            }

            return new ApiResponse<IEnumerable<PhysicalInventoryResponseDto>>
            {
                Success = true,
                Data = responseDtos,
                Message = $"{responseDtos.Count} conteos físicos obtenidos exitosamente",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PhysicalInventories with filters");
            return new ApiResponse<IEnumerable<PhysicalInventoryResponseDto>>
            {
                Success = false,
                Message = $"Error al obtener los conteos físicos: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // Método privado para mapear entidad a DTO
    private async Task<PhysicalInventoryResponseDto> MapToPhysicalInventoryResponseDto(PhysicalInventory physicalInventory)
    {
        // Obtener información adicional
        var user = await _userRepository.SelectByIdAsync(physicalInventory.CreatedBy);
        var details = await _physicalInventoryDetailRepository.GetByPhysicalInventoryIdAsync(physicalInventory.IdPhysicalInventory);
        var detailsList = details.ToList(); // Materializar para evitar enumeración múltiple

        return new PhysicalInventoryResponseDto
        {
            IdPhysicalInventory = physicalInventory.IdPhysicalInventory,
            InventoryDate = physicalInventory.InventoryDate,
            Status = physicalInventory.Status,
            CreatedBy = physicalInventory.CreatedBy,
            CreatedByUserName = user?.Name,
            Observations = physicalInventory.Observations,
            IdWarehouse = physicalInventory.IdWarehouse,
            WarehouseName = physicalInventory.Warehouse?.Name,
            WarehouseCode = physicalInventory.Warehouse?.Code,
            HasDetails = detailsList.Any(),
            DetailsCount = detailsList.Count,
            CreatedAt = physicalInventory.InventoryDate
        };
    }

    public async Task<ApiResponse<ProductInventoryListResponseDto>> GetProductInventoryListAsync(int idWarehouse, int userId)
    {
        try
        {
            _logger.LogInformation("Getting product inventory list for warehouse {WarehouseId} and user {UserId}", 
                idWarehouse, userId);

            // 1. Validar que el warehouse existe
            var warehouse = await _warehouseRepository.GetByIdAsync(idWarehouse);
            if (warehouse == null)
            {
                return new ApiResponse<ProductInventoryListResponseDto>
                {
                    Success = false,
                    Message = "El warehouse especificado no existe",
                    StatusCode = 404
                };
            }

            // 2. Obtener información del usuario para conocer su área
            var user = await _userRepository.SelectByIdAsync(userId);
            if (user == null)
            {
                return new ApiResponse<ProductInventoryListResponseDto>
                {
                    Success = false,
                    Message = "El usuario especificado no existe",
                    StatusCode = 404
                };
            }

            var userAreaId = user.Rol?.Area?.IdArea;
            var userAreaName = user.Rol?.Area?.Name ?? "Sin área";

            // 3. Intentar obtener productos desde la tabla Inventory filtrando por IdWarehouse y área
            var inventoryItems = await GetInventoryItemsWithLocationFilterAsync(idWarehouse, userAreaId);
            
            if (inventoryItems.Any())
            {
                // Hay registros en Inventory - usar estos datos
                var inventoryProducts = await MapInventoryToProductListAsync(inventoryItems, false);
                
                return new ApiResponse<ProductInventoryListResponseDto>
                {
                    Success = true,
                    Data = new ProductInventoryListResponseDto
                    {
                        Products = inventoryProducts,
                        TotalProducts = inventoryProducts.Count,
                        HasInventoryRecords = true,
                        WarehouseName = warehouse.Name,
                        UserAreaName = userAreaName
                    },
                    Message = $"Se encontraron {inventoryProducts.Count} productos en el inventario",
                    StatusCode = 200
                };
            }
            else
            {
                // No hay registros en Inventory - obtener productos directamente de la tabla Product
                _logger.LogInformation("No inventory records found, getting products directly from Product table");
                
                var directProducts = await GetDirectProductsAsync(idWarehouse);
                var productList = await MapProductsToProductListAsync(directProducts, true);
                
                return new ApiResponse<ProductInventoryListResponseDto>
                {
                    Success = true,
                    Data = new ProductInventoryListResponseDto
                    {
                        Products = productList,
                        TotalProducts = productList.Count,
                        HasInventoryRecords = false,
                        WarehouseName = warehouse.Name,
                        UserAreaName = userAreaName
                    },
                    Message = $"Se encontraron {productList.Count} productos directamente (sin registros de inventario)",
                    StatusCode = 200
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product inventory list for warehouse {WarehouseId}", idWarehouse);
            return new ApiResponse<ProductInventoryListResponseDto>
            {
                Success = false,
                Message = $"Error al obtener la lista de productos: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // Método auxiliar para obtener items de inventario con filtro de localización
    private async Task<List<AVASphere.ApplicationCore.Inventory.Entities.General.Inventory>> GetInventoryItemsWithLocationFilterAsync(int idWarehouse, int? userAreaId)
    {
        var inventoryItems = await _inventoryRepository.GetByWarehouseIdAsync(idWarehouse);
        
        // Si no hay área de usuario, devolver todos los items del warehouse
        if (!userAreaId.HasValue)
        {
            return inventoryItems.ToList();
        }

        // Filtrar por área si existe LocationDetail vinculado al área del usuario
        // Nota: Esto requiere una consulta más compleja debido a la estructura actual
        // Por ahora retornamos todos los items del warehouse
        // TODO: Implementar filtro por LocationDetails.IdArea cuando esté disponible
        return inventoryItems.ToList();
    }

    // Método auxiliar para obtener productos directamente
    private Task<IEnumerable<Product>> GetDirectProductsAsync(int idWarehouse)
    {
        // Obtener todos los productos (sin filtro por warehouse por ahora)
        // TODO: Implementar lógica para relacionar productos con warehouse
        // Por ahora simulamos obteniendo productos de un repositorio genérico
        
        // Como no tenemos repositorio de productos, vamos a simular con datos mínimos
        return Task.FromResult<IEnumerable<Product>>(new List<Product>());
    }

    // Método auxiliar para mapear Inventory a ProductInventoryListDto
    private async Task<List<ProductInventoryListDto>> MapInventoryToProductListAsync(
        List<AVASphere.ApplicationCore.Inventory.Entities.General.Inventory> inventoryItems, bool isDirectFromProducts)
    {
        var result = new List<ProductInventoryListDto>();

        foreach (var item in inventoryItems)
        {
            var productDto = new ProductInventoryListDto
            {
                IdPhysicalInventoryDetail = null, // No viene de PhysicalInventoryDetail
                SystemQuantity = item.Stock,
                PhysicalQuantity = 0, // Se llenará durante el conteo
                Difference = 0, // Se calculará después
                ProductMainName = item.Product.MainName ?? "Sin nombre",
                ProductUnit = item.Product.Unit ?? "Sin unidad", 
                ProductDescription = item.Product.Description ?? "Sin descripción",
                ProductSupplierName = item.Product.Supplier?.Name ?? "Sin proveedor",
                ProductCodeJsonCode = item.Product.CodeJson?.FirstOrDefault(c => c.Type == "Principal")?.Code ?? 
                                      item.Product.FirstCode,
                ProductProperties = await MapProductPropertiesAsync(item.Product.ProductProperties),
                IdLocationDetails = item.LocationDetail.HasValue ? (int?)item.LocationDetail.Value : null,
                LocationDetailsCode = await BuildLocationDetailsCodeAsync(item.LocationDetail),
                IsDirectFromProducts = isDirectFromProducts
            };

            result.Add(productDto);
        }

        return result;
    }

    // Método auxiliar para mapear Products directos a ProductInventoryListDto
    private async Task<List<ProductInventoryListDto>> MapProductsToProductListAsync(
        IEnumerable<Product> products, bool isDirectFromProducts)
    {
        var result = new List<ProductInventoryListDto>();

        foreach (var product in products)
        {
            var productDto = new ProductInventoryListDto
            {
                IdPhysicalInventoryDetail = null,
                SystemQuantity = 0, // No hay stock registrado
                PhysicalQuantity = 0,
                Difference = 0,
                ProductMainName = product.MainName ?? "Sin nombre",
                ProductUnit = product.Unit ?? "Sin unidad",
                ProductDescription = product.Description ?? "Sin descripción", 
                ProductSupplierName = product.Supplier?.Name ?? "Sin proveedor",
                ProductCodeJsonCode = product.CodeJson?.FirstOrDefault(c => c.Type == "Principal")?.Code ?? 
                                      product.FirstCode,
                ProductProperties = await MapProductPropertiesAsync(product.ProductProperties),
                IdLocationDetails = null,
                LocationDetailsCode = "Sin ubicación",
                IsDirectFromProducts = isDirectFromProducts
            };

            result.Add(productDto);
        }

        return result;
    }

    // Método auxiliar para mapear propiedades del producto
    private Task<List<ProductPropertyDto>> MapProductPropertiesAsync(ICollection<ProductProperties> properties)
    {
        var result = properties.Select(p => new ProductPropertyDto
        {
            IdProductProperties = p.IdProductProperties,
            CustomValue = p.CustomValue,
            IdPropertyValue = p.IdPropertyValue,
            PropertyName = p.PropertyValue?.Property?.Name ?? "Sin nombre",
            PropertyValue = p.PropertyValue?.Value ?? "Sin valor"
        }).ToList();

        return Task.FromResult(result);
    }

    // Método auxiliar para construir el código de ubicación
    private Task<string> BuildLocationDetailsCodeAsync(double? locationDetailId)
    {
        if (!locationDetailId.HasValue)
        {
            return Task.FromResult("Sin ubicación");
        }

        // TODO: Implementar lógica para obtener LocationDetails por ID y construir el código
        // Por ahora retornamos un placeholder
        return Task.FromResult($"LOC-{locationDetailId.Value}");
    }
}