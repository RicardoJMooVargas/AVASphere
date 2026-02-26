using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Enums;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.Inventory.Services;

public class PhysicalInventoryService : IPhysicalInventoryService
{
    private readonly IPhysicalInventoryRepository _physicalInventoryRepository;
    private readonly IPhysicalInventoryDetailRepository _physicalInventoryDetailRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILocationDetailsRepository _locationDetailsRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<PhysicalInventoryService> _logger;

    public PhysicalInventoryService(
        IPhysicalInventoryRepository physicalInventoryRepository,
        IPhysicalInventoryDetailRepository physicalInventoryDetailRepository,
        IWarehouseRepository warehouseRepository,
        IInventoryRepository inventoryRepository,
        ILocationDetailsRepository locationDetailsRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        ILogger<PhysicalInventoryService> logger)
    {
        _physicalInventoryRepository = physicalInventoryRepository;
        _physicalInventoryDetailRepository = physicalInventoryDetailRepository;
        _warehouseRepository = warehouseRepository;
        _inventoryRepository = inventoryRepository;
        _locationDetailsRepository = locationDetailsRepository;
        _productRepository = productRepository;
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
                Status = PhysicalInventoryStatus.Open.ToString(),
                CreatedBy = userId,
                Observations = createDto.Observations,
                IdWarehouse = createDto.IdWarehouse
            };

            // Crear en la base de datos
            var createdInventory = await _physicalInventoryRepository.CreateAsync(physicalInventory);

            // Generar automáticamente los PhysicalInventoryDetail basado en filtros
            await GeneratePhysicalInventoryDetailsAsync(createdInventory.IdPhysicalInventory, createDto.IdWarehouse, 
                user.Rol?.Area?.IdArea, createDto.ProductFilters);

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

            if (existingInventory.IdWarehouse != updateDto.IdWarehouse)
            {
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

            existingInventory.InventoryDate = updateDto.InventoryDate;
            existingInventory.Status = updateDto.Status;
            existingInventory.CreatedBy = updateDto.CreatedBy;
            existingInventory.Observations = updateDto.Observations;
            existingInventory.IdWarehouse = updateDto.IdWarehouse;

            var updatedInventory = await _physicalInventoryRepository.UpdateAsync(existingInventory);
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

            var details = await _physicalInventoryDetailRepository.GetByPhysicalInventoryIdAsync(idPhysicalInventory);
            var detailsList = details.ToList();
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
            // Implementación básica - puede expandirse según necesidades
            var physicalInventory = await _physicalInventoryRepository.GetByIdAsync(idPhysicalInventory);
            if (physicalInventory == null)
            {
                return new ApiResponse<PhysicalInventoryWithProductsDto>
                {
                    Success = false,
                    Message = "El conteo físico especificado no existe",
                    StatusCode = 404
                };
            }

            // Retorna respuesta básica - implementar según PhysicalInventoryWithProductsDto
            return new ApiResponse<PhysicalInventoryWithProductsDto>
            {
                Success = true,
                Message = "Método no implementado completamente",
                StatusCode = 501
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

    public async Task<ApiResponse<ProductInventoryListResponseDto>> GetProductInventoryListAsync(int idPhysicalInventory, int userId)
    {
        try
        {
            _logger.LogInformation("Getting product inventory list for PhysicalInventory {PhysicalInventoryId} and user {UserId}", 
                idPhysicalInventory, userId);

            var physicalInventory = await _physicalInventoryRepository.GetByIdAsync(idPhysicalInventory);
            if (physicalInventory == null)
            {
                return new ApiResponse<ProductInventoryListResponseDto>
                {
                    Success = false,
                    Message = "El inventario físico especificado no existe",
                    StatusCode = 404
                };
            }

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

            var userAreaName = user.Rol?.Area?.Name ?? "Sin área";

            var inventoryDetails = await _physicalInventoryDetailRepository.GetByPhysicalInventoryIdAsync(idPhysicalInventory);
            var detailsList = inventoryDetails.ToList();

            if (!detailsList.Any())
            {
                return new ApiResponse<ProductInventoryListResponseDto>
                {
                    Success = true,
                    Data = new ProductInventoryListResponseDto
                    {
                        Products = new List<ProductInventoryListDto>(),
                        TotalProducts = 0,
                        HasInventoryRecords = true,
                        WarehouseName = physicalInventory.Warehouse?.Name ?? "Sin nombre",
                        UserAreaName = userAreaName
                    },
                    Message = "No hay productos asociados a este inventario físico",
                    StatusCode = 200
                };
            }

            var productList = await MapPhysicalInventoryDetailsToProductListAsync(detailsList);

            return new ApiResponse<ProductInventoryListResponseDto>
            {
                Success = true,
                Data = new ProductInventoryListResponseDto
                {
                    Products = productList,
                    TotalProducts = productList.Count,
                    HasInventoryRecords = true,
                    WarehouseName = physicalInventory.Warehouse?.Name ?? "Sin nombre",
                    UserAreaName = userAreaName
                },
                Message = $"Se encontraron {productList.Count} productos en el inventario físico",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product inventory list for PhysicalInventory {PhysicalInventoryId}", idPhysicalInventory);
            return new ApiResponse<ProductInventoryListResponseDto>
            {
                Success = false,
                Message = $"Error al obtener la lista de productos: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<ApiResponse<ProductInventoryListPaginatedResponseDto>> GetProductInventoryListPaginatedAsync(
        int idPhysicalInventory, 
        int userId, 
        ProductInventoryListPaginationDto pagination, 
        ProductInventoryListFiltersDto? filters = null)
    {
        try
        {
            _logger.LogInformation("Getting paginated product inventory list for PhysicalInventory {PhysicalInventoryId}, user {UserId}, page {PageNumber}, size {PageSize}", 
                idPhysicalInventory, userId, pagination.PageNumber, pagination.PageSize);

            // Validaciones iniciales
            var physicalInventory = await _physicalInventoryRepository.GetByIdAsync(idPhysicalInventory);
            if (physicalInventory == null)
            {
                return new ApiResponse<ProductInventoryListPaginatedResponseDto>
                {
                    Success = false,
                    Message = "El inventario físico especificado no existe",
                    StatusCode = 404
                };
            }

            var user = await _userRepository.SelectByIdAsync(userId);
            if (user == null)
            {
                return new ApiResponse<ProductInventoryListPaginatedResponseDto>
                {
                    Success = false,
                    Message = "El usuario especificado no existe",
                    StatusCode = 404
                };
            }

            var userAreaName = user.Rol?.Area?.Name ?? "Sin área";

            // Obtener todos los detalles del inventario físico
            var inventoryDetails = await _physicalInventoryDetailRepository.GetByPhysicalInventoryIdAsync(idPhysicalInventory);
            var allDetailsList = inventoryDetails.ToList();

            // Generar catálogos para filtros
            var catalogs = GenerateInventoryCatalogsAsync(allDetailsList);

            // Aplicar filtros
            var filteredDetails = ApplyInventoryFilters(allDetailsList, filters);
            var totalFilteredCount = filteredDetails.Count;

            // Aplicar paginación
            var paginatedDetails = filteredDetails
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            // Mapear a DTOs
            var productList = await MapPhysicalInventoryDetailsToProductListAsync(paginatedDetails);

            // Calcular metadatos de paginación
            var totalPages = (int)Math.Ceiling((double)totalFilteredCount / pagination.PageSize);

            var paginationMetadata = new PaginationMetadataDto
            {
                CurrentPage = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalFilteredCount,
                TotalPages = totalPages,
                HasPrevious = pagination.PageNumber > 1,
                HasNext = pagination.PageNumber < totalPages
            };

            var inventoryInfo = new ProductInventoryInfoDto
            {
                HasInventoryRecords = allDetailsList.Any(),
                WarehouseName = physicalInventory.Warehouse?.Name ?? "Sin nombre",
                UserAreaName = userAreaName,
                TotalProductsInInventory = allDetailsList.Count,
                FilteredProductsCount = totalFilteredCount
            };

            var response = new ProductInventoryListPaginatedResponseDto
            {
                Products = productList,
                Pagination = paginationMetadata,
                Catalogs = catalogs,
                InventoryInfo = inventoryInfo
            };

            return new ApiResponse<ProductInventoryListPaginatedResponseDto>
            {
                Success = true,
                Data = response,
                Message = $"Se encontraron {totalFilteredCount} productos (página {pagination.PageNumber} de {totalPages})",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated product inventory list for PhysicalInventory {PhysicalInventoryId}", idPhysicalInventory);
            return new ApiResponse<ProductInventoryListPaginatedResponseDto>
            {
                Success = false,
                Message = $"Error al obtener la lista paginada de productos: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<ApiResponse<PhysicalInventoryCountResponseDto>> CreateOrUpdatePhysicalCountAsync(CreateOrUpdatePhysicalCountDto countDto)
    {
        try
        {
            _logger.LogInformation("Creating or updating physical count for Product {ProductId} in PhysicalInventory {PhysicalInventoryId}", 
                countDto.IdProduct, countDto.IdPhysicalInventory);

            var physicalInventory = await _physicalInventoryRepository.GetByIdAsync(countDto.IdPhysicalInventory);
            if (physicalInventory == null)
            {
                return new ApiResponse<PhysicalInventoryCountResponseDto>
                {
                    Success = false,
                    Message = "El inventario físico especificado no existe",
                    StatusCode = 404
                };
            }

            var existingDetail = await _physicalInventoryDetailRepository.GetByPhysicalInventoryAndProductAsync(
                countDto.IdPhysicalInventory, countDto.IdProduct);

            PhysicalInventoryDetail detail;
            bool isNewRecord = false;

            if (existingDetail == null)
            {
                isNewRecord = true;
                
                double systemQuantity = 0;
                var inventoryRecord = await _inventoryRepository.GetByWarehouseAndProductAsync(
                    physicalInventory.IdWarehouse, countDto.IdProduct);
                
                if (inventoryRecord != null)
                {
                    systemQuantity = inventoryRecord.Stock;
                }

                // Validar IdLocationDetails si se proporciona
                int? validLocationDetailsId = null;
                if (countDto.IdLocationDetails.HasValue && countDto.IdLocationDetails.Value > 0)
                {
                    var locationDetails = await _locationDetailsRepository.GetByIdAsync(countDto.IdLocationDetails.Value);
                    if (locationDetails != null)
                    {
                        validLocationDetailsId = countDto.IdLocationDetails;
                    }
                    else
                    {
                        _logger.LogWarning("LocationDetails with ID {LocationDetailsId} not found, setting to null", countDto.IdLocationDetails);
                    }
                }

                detail = new PhysicalInventoryDetail
                {
                    IdPhysicalInventory = countDto.IdPhysicalInventory,
                    IdProduct = countDto.IdProduct,
                    IdLocationDetails = validLocationDetailsId,
                    SystemQuantity = systemQuantity,
                    PhysicalQuantity = countDto.PhysicalQuantity,
                    Difference = countDto.PhysicalQuantity - systemQuantity
                };

                detail = await _physicalInventoryDetailRepository.CreateAsync(detail);
            }
            else
            {
                existingDetail.PhysicalQuantity = countDto.PhysicalQuantity;
                existingDetail.Difference = countDto.PhysicalQuantity - existingDetail.SystemQuantity;
                
                // Validar IdLocationDetails si se proporciona
                if (countDto.IdLocationDetails.HasValue && countDto.IdLocationDetails.Value > 0)
                {
                    var locationDetails = await _locationDetailsRepository.GetByIdAsync(countDto.IdLocationDetails.Value);
                    if (locationDetails != null)
                    {
                        existingDetail.IdLocationDetails = countDto.IdLocationDetails;
                    }
                    else
                    {
                        _logger.LogWarning("LocationDetails with ID {LocationDetailsId} not found, keeping existing value", countDto.IdLocationDetails);
                    }
                }

                detail = await _physicalInventoryDetailRepository.UpdateAsync(existingDetail);
            }

            var response = new PhysicalInventoryCountResponseDto
            {
                IdPhysicalInventoryDetail = detail.IdPhysicalInventoryDetail,
                SystemQuantity = detail.SystemQuantity,
                PhysicalQuantity = detail.PhysicalQuantity,
                Difference = detail.Difference,
                IdPhysicalInventory = detail.IdPhysicalInventory,
                IdProduct = detail.IdProduct,
                ProductMainName = detail.Product?.MainName ?? "Producto no encontrado",
                IdLocationDetails = detail.IdLocationDetails,
                LocationDetailsCode = await BuildLocationDetailsCodeAsync(detail.IdLocationDetails),
                IsNewRecord = isNewRecord
            };

            return new ApiResponse<PhysicalInventoryCountResponseDto>
            {
                Success = true,
                Data = response,
                Message = isNewRecord ? "Conteo físico creado exitosamente" : "Conteo físico actualizado exitosamente",
                StatusCode = isNewRecord ? 201 : 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating or updating physical count for Product {ProductId}", countDto.IdProduct);
            return new ApiResponse<PhysicalInventoryCountResponseDto>
            {
                Success = false,
                Message = $"Error al procesar el conteo físico: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // Métodos privados auxiliares
    private async Task<PhysicalInventoryResponseDto> MapToPhysicalInventoryResponseDto(PhysicalInventory physicalInventory)
    {
        var user = await _userRepository.SelectByIdAsync(physicalInventory.CreatedBy);
        var details = await _physicalInventoryDetailRepository.GetByPhysicalInventoryIdAsync(physicalInventory.IdPhysicalInventory);
        var detailsList = details.ToList();

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

    private async Task<List<ProductInventoryListDto>> MapPhysicalInventoryDetailsToProductListAsync(
        List<PhysicalInventoryDetail> details)
    {
        var result = new List<ProductInventoryListDto>();

        foreach (var detail in details)
        {
            var principalCode = GetPrincipalCodeFromProduct(detail.Product);
            
            var productDto = new ProductInventoryListDto
            {
                IdPhysicalInventoryDetail = detail.IdPhysicalInventoryDetail,
                SystemQuantity = detail.SystemQuantity,
                PhysicalQuantity = detail.PhysicalQuantity,
                Difference = detail.Difference,
                ProductMainName = detail.Product.MainName ?? "Sin nombre",
                ProductUnit = detail.Product.Unit ?? "Sin unidad", 
                ProductDescription = detail.Product.Description ?? "Sin descripción",
                ProductSupplierName = detail.Product.Supplier?.Name ?? "Sin proveedor",
                ProductCodeJsonCode = principalCode,
                ProductProperties = MapProductPropertiesToDto(detail.Product.ProductProperties),
                IdLocationDetails = detail.IdLocationDetails,
                LocationDetailsCode = await BuildLocationDetailsCodeAsync(detail.IdLocationDetails),
                IsDirectFromProducts = false
            };

            result.Add(productDto);
        }

        return result;
    }

    private string GetPrincipalCodeFromProduct(Product product)
    {
        try
        {
            var principalCode = product.CodeJson?.FirstOrDefault(c => c.Type == "Principal")?.Code;
            return principalCode ?? product.FirstCode ?? "Sin código";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting principal code for product {ProductId}", product.IdProduct);
            return "Error código";
        }
    }

    private List<AVASphere.ApplicationCore.Inventory.DTOs.ProductPropertyDto> MapProductPropertiesToDto(ICollection<ProductProperties> properties)
    {
        try 
        {
            return properties.Select(p => new AVASphere.ApplicationCore.Inventory.DTOs.ProductPropertyDto
            {
                IdProductProperties = p.IdProductProperties,
                CustomValue = p.CustomValue,
                IdPropertyValue = p.IdPropertyValue,
                PropertyName = p.PropertyValue?.Property?.Name ?? "Sin nombre",
                PropertyValue = p.PropertyValue?.Value ?? "Sin valor"
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping product properties");
            return new List<AVASphere.ApplicationCore.Inventory.DTOs.ProductPropertyDto>();
        }
    }

    private async Task<string> BuildLocationDetailsCodeAsync(int? locationDetailId)
    {
        if (!locationDetailId.HasValue || locationDetailId.Value == 0)
        {
            return "DESCONOCIDO";
        }

        try 
        {
            var locationDetails = await _locationDetailsRepository.GetByIdAsync(locationDetailId.Value);
            
            if (locationDetails == null)
            {
                return "DESCONOCIDO ER";
            }

            var codeRack = locationDetails.StorageStructure?.CodeRack ?? "SIN-RACK";
            var section = locationDetails.Section ?? "SIN-SECCION";
            var verticalLevel = locationDetails.VerticalLevel.ToString();
            
            return $"{codeRack}-{section}-{verticalLevel}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building location details code for ID: {LocationDetailId}", locationDetailId);
            return "DESCONOCIDO ER";
        }
    }

    /// <summary>
    /// Genera los catálogos de proveedores, familia, clase y línea para los filtros
    /// </summary>
    private ProductInventoryCatalogsDto GenerateInventoryCatalogsAsync(List<PhysicalInventoryDetail> details)
    {
        try
        {
            var catalogs = new ProductInventoryCatalogsDto();

            if (!details.Any())
            {
                return catalogs;
            }

            // Obtener proveedores únicos
            var suppliers = details
                .Where(d => d.Product?.Supplier != null)
                .Select(d => new AVASphere.ApplicationCore.Inventory.DTOs.SupplierFilterDto
                {
                    IdSupplier = d.Product!.Supplier!.IdSupplier,
                    Name = d.Product.Supplier.Name ?? "Sin nombre",
                    CompanyName = d.Product.Supplier.CompanyName ?? "Sin empresa"
                })
                .GroupBy(s => s.IdSupplier)
                .Select(g => g.First())
                .OrderBy(s => s.Name)
                .ToList();

            catalogs.Suppliers = suppliers;

            // Obtener propiedades únicas (Familia, Clase, Línea)
            var allProperties = details
                .Where(d => d.Product?.ProductProperties != null)
                .SelectMany(d => d.Product!.ProductProperties)
                .Where(pp => pp.PropertyValue?.Property != null)
                .ToList();

            // Familia
            var familias = allProperties
                .Where(pp => string.Equals(pp.PropertyValue!.Property!.Name, "Familia", StringComparison.OrdinalIgnoreCase))
                .Select(pp => pp.CustomValue ?? pp.PropertyValue!.Value ?? "Sin valor")
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();

            catalogs.Familias = familias;

            // Clase
            var clases = allProperties
                .Where(pp => string.Equals(pp.PropertyValue!.Property!.Name, "Clase", StringComparison.OrdinalIgnoreCase))
                .Select(pp => pp.CustomValue ?? pp.PropertyValue!.Value ?? "Sin valor")
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();

            catalogs.Clases = clases;

            // Línea
            var lineas = allProperties
                .Where(pp => string.Equals(pp.PropertyValue!.Property!.Name, "Línea", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(pp.PropertyValue!.Property!.Name, "Linea", StringComparison.OrdinalIgnoreCase))
                .Select(pp => pp.CustomValue ?? pp.PropertyValue!.Value ?? "Sin valor")
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();

            catalogs.Lineas = lineas;

            return catalogs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory catalogs");
            return new ProductInventoryCatalogsDto();
        }
    }

    /// <summary>
    /// Aplica filtros a la lista de detalles de inventario físico
    /// </summary>
    private List<PhysicalInventoryDetail> ApplyInventoryFilters(
        List<PhysicalInventoryDetail> details, 
        ProductInventoryListFiltersDto? filters)
    {
        if (filters == null)
        {
            return details;
        }

        var filtered = details.AsEnumerable();

        // Filtro por texto de búsqueda (nombre o descripción del producto)
        if (!string.IsNullOrWhiteSpace(filters.SearchText))
        {
            var searchText = filters.SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(d =>
                (d.Product?.MainName?.ToLowerInvariant().Contains(searchText) == true) ||
                (d.Product?.Description?.ToLowerInvariant().Contains(searchText) == true));
        }

        // Filtro por proveedor
        if (filters.IdSupplier.HasValue)
        {
            filtered = filtered.Where(d => d.Product?.IdSupplier == filters.IdSupplier.Value);
        }

        // Filtro por familia
        if (!string.IsNullOrWhiteSpace(filters.Familia))
        {
            var familiaFilter = filters.Familia.Trim();
            filtered = filtered.Where(d =>
                d.Product?.ProductProperties?.Any(pp =>
                    string.Equals(pp.PropertyValue?.Property?.Name, "Familia", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(pp.CustomValue ?? pp.PropertyValue?.Value, familiaFilter, StringComparison.OrdinalIgnoreCase)
                ) == true);
        }

        // Filtro por clase
        if (!string.IsNullOrWhiteSpace(filters.Clase))
        {
            var claseFilter = filters.Clase.Trim();
            filtered = filtered.Where(d =>
                d.Product?.ProductProperties?.Any(pp =>
                    string.Equals(pp.PropertyValue?.Property?.Name, "Clase", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(pp.CustomValue ?? pp.PropertyValue?.Value, claseFilter, StringComparison.OrdinalIgnoreCase)
                ) == true);
        }

        // Filtro por línea
        if (!string.IsNullOrWhiteSpace(filters.Linea))
        {
            var lineaFilter = filters.Linea.Trim();
            filtered = filtered.Where(d =>
                d.Product?.ProductProperties?.Any(pp =>
                    (string.Equals(pp.PropertyValue?.Property?.Name, "Línea", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(pp.PropertyValue?.Property?.Name, "Linea", StringComparison.OrdinalIgnoreCase)) &&
                    string.Equals(pp.CustomValue ?? pp.PropertyValue?.Value, lineaFilter, StringComparison.OrdinalIgnoreCase)
                ) == true);
        }

        return filtered.ToList();
    }

    private async Task GeneratePhysicalInventoryDetailsAsync(int idPhysicalInventory, int idWarehouse, 
        int? userAreaId, ProductSelectionFilterDto? filters)
    {
        try
        {
            _logger.LogInformation("Generating PhysicalInventoryDetails for PhysicalInventory {PhysicalInventoryId} with filters: {@Filters}", 
                idPhysicalInventory, filters);

            // Verificar que el PhysicalInventory existe
            var physicalInventory = await _physicalInventoryRepository.GetByIdAsync(idPhysicalInventory);
            if (physicalInventory == null)
            {
                throw new InvalidOperationException($"PhysicalInventory with ID {idPhysicalInventory} does not exist");
            }

            var detailsToCreate = new List<PhysicalInventoryDetail>();

            if (filters != null && (filters.IdSupplier.HasValue || !string.IsNullOrEmpty(filters.SupplierName) || 
                filters.ProductProperties?.Any() == true))
            {
                _logger.LogInformation("Using filtered products with supplier ID: {SupplierId}, supplier name: {SupplierName}", 
                    filters.IdSupplier, filters.SupplierName);

                var productFilter = new ProductFilterDto
                {
                    IdSupplier = filters.IdSupplier,
                    SupplierName = filters.SupplierName,
                    Properties = filters.ProductProperties
                };

                var filteredProducts = await _productRepository.GetAllProductsAsync(productFilter);
                var filteredProductsList = filteredProducts.ToList();
                
                _logger.LogInformation("Found {ProductCount} filtered products", filteredProductsList.Count);
                
                foreach (var product in filteredProductsList)
                {
                    try
                    {
                        _logger.LogDebug("Processing product {ProductId}: {ProductName}", 
                            product.IdProduct, product.MainName ?? "Sin nombre");

                        // Verificar que el producto no tenga ya un detalle para este inventario físico
                        var existingDetail = await _physicalInventoryDetailRepository
                            .GetByPhysicalInventoryAndProductAsync(idPhysicalInventory, product.IdProduct);
                        
                        if (existingDetail != null)
                        {
                            _logger.LogWarning("Product {ProductId} already has a detail for PhysicalInventory {PhysicalInventoryId}, skipping", 
                                product.IdProduct, idPhysicalInventory);
                            continue;
                        }

                        var inventoryRecord = await _inventoryRepository.GetByWarehouseAndProductAsync(idWarehouse, product.IdProduct);
                        double systemQuantity = inventoryRecord?.Stock ?? 0;
                        
                        // Para conteo físico inicial, LocationDetails siempre es null
                        // Se asignará durante el proceso de conteo
                        var detail = new PhysicalInventoryDetail
                        {
                            IdPhysicalInventory = idPhysicalInventory,
                            IdProduct = product.IdProduct,
                            IdLocationDetails = null, // Siempre null en la creación inicial
                            SystemQuantity = systemQuantity,
                            PhysicalQuantity = 0,
                            Difference = -systemQuantity
                        };
                        
                        detailsToCreate.Add(detail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing product {ProductId} for PhysicalInventory {PhysicalInventoryId}", 
                            product.IdProduct, idPhysicalInventory);
                        throw;
                    }
                }
            }
            else
            {
                _logger.LogInformation("Using all inventory items for warehouse {WarehouseId}", idWarehouse);
                
                var inventoryItems = await _inventoryRepository.GetByWarehouseIdAsync(idWarehouse);
                var inventoryItemsList = inventoryItems.ToList();

                if (inventoryItemsList.Any())
                {
                    _logger.LogInformation("Found {InventoryItemCount} inventory items", inventoryItemsList.Count);
                    
                    foreach (var inventoryItem in inventoryItemsList)
                    {
                        // Verificar que el producto no tenga ya un detalle para este inventario físico
                        var existingDetail = await _physicalInventoryDetailRepository
                            .GetByPhysicalInventoryAndProductAsync(idPhysicalInventory, inventoryItem.IdProduct);
                        
                        if (existingDetail != null)
                        {
                            _logger.LogWarning("Product {ProductId} already has a detail for PhysicalInventory {PhysicalInventoryId}, skipping", 
                                inventoryItem.IdProduct, idPhysicalInventory);
                            continue;
                        }

                        // Para conteo físico inicial, LocationDetails siempre es null
                        // Se asignará durante el proceso de conteo
                        var detail = new PhysicalInventoryDetail
                        {
                            IdPhysicalInventory = idPhysicalInventory,
                            IdProduct = inventoryItem.IdProduct,
                            IdLocationDetails = null, // Siempre null en la creación inicial
                            SystemQuantity = inventoryItem.Stock,
                            PhysicalQuantity = 0,
                            Difference = -inventoryItem.Stock
                        };
                        
                        detailsToCreate.Add(detail);
                    }
                }
                else
                {
                    _logger.LogInformation("No inventory records found, creating details from all products");
                    
                    var allProducts = await _productRepository.GetAllProductsAsync();
                    var allProductsList = allProducts.ToList();
                    
                    _logger.LogInformation("Found {AllProductCount} total products", allProductsList.Count);
                    
                    foreach (var product in allProductsList)
                    {
                        // Verificar que el producto no tenga ya un detalle para este inventario físico
                        var existingDetail = await _physicalInventoryDetailRepository
                            .GetByPhysicalInventoryAndProductAsync(idPhysicalInventory, product.IdProduct);
                        
                        if (existingDetail != null)
                        {
                            _logger.LogWarning("Product {ProductId} already has a detail for PhysicalInventory {PhysicalInventoryId}, skipping", 
                                product.IdProduct, idPhysicalInventory);
                            continue;
                        }

                        var detail = new PhysicalInventoryDetail
                        {
                            IdPhysicalInventory = idPhysicalInventory,
                            IdProduct = product.IdProduct,
                            IdLocationDetails = null,
                            SystemQuantity = 0,
                            PhysicalQuantity = 0,
                            Difference = 0
                        };
                        
                        detailsToCreate.Add(detail);
                    }
                }
            }

            _logger.LogInformation("About to create {Count} PhysicalInventoryDetails", detailsToCreate.Count);

            if (detailsToCreate.Any())
            {
                // Validar que no hay duplicados en la lista
                var duplicates = detailsToCreate
                    .GroupBy(d => new { d.IdPhysicalInventory, d.IdProduct })
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (duplicates.Any())
                {
                    _logger.LogWarning("Found {DuplicateCount} duplicate combinations of PhysicalInventory-Product in the creation list", 
                        duplicates.Count);
                    
                    // Remover duplicados, manteniendo solo el primero
                    detailsToCreate = detailsToCreate
                        .GroupBy(d => new { d.IdPhysicalInventory, d.IdProduct })
                        .Select(g => g.First())
                        .ToList();
                    
                    _logger.LogInformation("After removing duplicates: {Count} PhysicalInventoryDetails", 
                        detailsToCreate.Count);
                }

                await _physicalInventoryDetailRepository.CreateBatchAsync(detailsToCreate);
                _logger.LogInformation("Successfully created {Count} PhysicalInventoryDetails for PhysicalInventory {PhysicalInventoryId}", 
                    detailsToCreate.Count, idPhysicalInventory);
            }
            else
            {
                _logger.LogWarning("No products found to create PhysicalInventoryDetails for PhysicalInventory {PhysicalInventoryId}", 
                    idPhysicalInventory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PhysicalInventoryDetails for PhysicalInventory {PhysicalInventoryId}: {ErrorMessage}", 
                idPhysicalInventory, ex.Message);
            
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "Inner exception: {InnerErrorMessage}", ex.InnerException.Message);
            }
            
            throw;
        }
    }
}
