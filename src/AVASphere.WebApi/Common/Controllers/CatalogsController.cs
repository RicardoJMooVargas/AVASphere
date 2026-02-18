﻿using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using AVASphere.ApplicationCore.Projects.DTOs;
using AVASphere.ApplicationCore.Projects.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Common.Controllers;

[ApiController]
[Route("api/common/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Common))]
[Tags("Common - Catalogs")]
public class CatalogsController : ControllerBase
{
    private readonly IAreaService _areaService;
    private readonly IProjectCategoryService _projectCategoryService;
    private readonly IPropertyService _propertyService;
    private readonly IPropertyValueService _propertyValueService;
    private readonly ISupplierService _supplierService;
    private readonly IWarehouseService _warehouseService;

    public CatalogsController(IAreaService areaService, IProjectCategoryService projectCategoryService, IPropertyService propertyService,
        IPropertyValueService propertyValueService, ISupplierService supplierService, IWarehouseService warehouseService)
    {
        _areaService = areaService;
        _propertyService = propertyService;
        _propertyValueService = propertyValueService;
        _projectCategoryService = projectCategoryService;
        _supplierService = supplierService;
        _warehouseService = warehouseService;
    }

    //Area Controller

    [HttpPost("new-area")]
    public async Task<ActionResult> NewArea([FromBody] AreaRequestDto areaRequest)
    {
        try
        {
            var createdArea = await _areaService.CreateAsync(areaRequest);

            return CreatedAtAction(nameof(GetAreas), new { id = createdArea.IdArea },
                new ApiResponse(createdArea, "Area created successfully", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpGet("get-areas")]
    public async Task<ActionResult> GetAreas([FromQuery] int? id, [FromQuery] string? name)
    {
        try
        {
            if (id.HasValue)
            {
                var area = await _areaService.GetByIdAsync(id.Value);
                if (area == null)
                    return NotFound(new ApiResponse($"Area with ID {id} not found", 404));

                return Ok(new ApiResponse(area, "Area retrieved successfully", 200));
            }

            if (!string.IsNullOrEmpty(name))
            {
                var area = await _areaService.GetByNameAsync(name);
                if (area == null)
                    return NotFound(new ApiResponse($"Area with name '{name}' not found", 404));

                return Ok(new ApiResponse(area, "Area retrieved successfully", 200));
            }

            var areas = await _areaService.GetAllAsync();
            return Ok(new ApiResponse(areas, "Areas retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit-areas/{id}")]
    public async Task<ActionResult> UpdateArea(int id, [FromBody] AreaRequestDto areaRequest)
    {
        try
        {
            var updatedArea = await _areaService.UpdateAsync(id, areaRequest);
            return Ok(new ApiResponse(updatedArea, "Area updated successfully", 200));
        }
        catch (KeyNotFoundException keyEx)
        {
            return NotFound(new ApiResponse(keyEx.Message, 404));
        }
        catch (InvalidOperationException opEx)
        {
            return BadRequest(new ApiResponse(opEx.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("delete-areas/{id}")]
    public async Task<ActionResult> DeleteArea(int id)
    {
        try
        {
            var result = await _areaService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Area with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Area deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }


    //Project Category Controller

    [HttpPost("new-projectCategory")]
    public async Task<ActionResult> NewProjectCategory([FromBody] ProjectCategoryRequestDto projectCategoryRequest)
    {
        try
        {
            var createdProjectCategory = await _projectCategoryService.CreateAsync(projectCategoryRequest);

            return CreatedAtAction(nameof(GetProjectCategories), new { id = createdProjectCategory.IdProjectCategory },
                new ApiResponse(createdProjectCategory, "Project Category created successfully", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpGet("get-projectCategory")]
    public async Task<ActionResult> GetProjectCategories([FromQuery] int? id, [FromQuery] string? name)
    {
        try
        {
            if (id.HasValue)
            {
                var projectCategory = await _projectCategoryService.GetByIdAsync(id.Value);
                if (projectCategory == null)
                    return NotFound(new ApiResponse($"Project category with ID {id} not found", 404));

                return Ok(new ApiResponse(projectCategory, "Project category retrieved successfully", 200));
            }

            if (!string.IsNullOrEmpty(name))
            {
                var projectCategory = await _projectCategoryService.GetByNameAsync(name);
                if (projectCategory == null)
                    return NotFound(new ApiResponse($"Project category with name '{name}' not found", 404));

                return Ok(new ApiResponse(projectCategory, "Project category retrieved successfully", 200));
            }

            var projectCategories = await _projectCategoryService.GetAllAsync();
            return Ok(new ApiResponse(projectCategories, "Project categories retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit-projectCategory/{id}")]
    public async Task<ActionResult> UpdateProjectCategory(int id,
        [FromBody] ProjectCategoryRequestDto projectCategoryRequest)
    {
        try
        {
            var updatedProjectCategory = await _projectCategoryService.UpdateAsync(id, projectCategoryRequest);
            return Ok(new ApiResponse(updatedProjectCategory, "Project Category updated successfully", 200));
        }
        catch (KeyNotFoundException keyEx)
        {
            return NotFound(new ApiResponse(keyEx.Message, 404));
        }
        catch (InvalidOperationException opEx)
        {
            return BadRequest(new ApiResponse(opEx.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    /*[HttpDelete("delete-projectCategory/{id}")]
    public async Task<ActionResult> DeleteProjectCategory(int id)
    {
        try
        {
            _logger.LogInformation("Deleting project category with ID: {ProjectCategoryId}", id);
            var result = await _projectCategoryService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Project category with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Project category deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while deleting project category: {ProjectCategoryId}", id);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project category with ID: {ProjectCategoryId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }*/

    // Property Controller

    [HttpPost("new-property")]
    public async Task<ActionResult> NewProperty([FromBody] PropertyRequestDto propertyRequest)
    {
        try
        {
            var createdProperty = await _propertyService.CreateAsync(propertyRequest);

            return CreatedAtAction(nameof(GetProperties), new { id = createdProperty.IdProperty },
                new ApiResponse(createdProperty, "Property created successfully", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpGet("get-property")]
    public async Task<ActionResult> GetProperties([FromQuery] int? id, [FromQuery] string? name)
    {
        try
        {
            if (id.HasValue)
            {
                var property = await _propertyService.GetByIdAsync(id.Value);
                if (property == null)
                    return NotFound(new ApiResponse($"Property with ID {id} not found", 404));

                return Ok(new ApiResponse(property, "Property retrieved successfully", 200));
            }

            if (!string.IsNullOrEmpty(name))
            {
                var property = await _propertyService.GetByNameAsync(name);
                if (property == null)
                    return NotFound(new ApiResponse($"Property with name '{name}' not found", 404));

                return Ok(new ApiResponse(property, "Property retrieved successfully", 200));
            }

            var properties = await _propertyService.GetAllAsync();
            return Ok(new ApiResponse(properties, "Property retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit-property/{id}")]
    public async Task<ActionResult> UpdateProperty(int id,
        [FromBody] PropertyRequestDto propertyRequest)
    {
        try
        {
            var updatedProperty = await _propertyService.UpdateAsync(id, propertyRequest);
            return Ok(new ApiResponse(updatedProperty, "Property updated successfully", 200));
        }
        catch (KeyNotFoundException keyEx)
        {
            return NotFound(new ApiResponse(keyEx.Message, 404));
        }
        catch (InvalidOperationException opEx)
        {
            return BadRequest(new ApiResponse(opEx.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("delete-property/{id}")]
    public async Task<ActionResult> DeleteProperty(int id)
    {
        try
        {
            var result = await _propertyService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Property with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Property deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    // PropertyValue Controller

    [HttpGet("get-propertyValue")]
    public async Task<ActionResult> GetPropertyValues(
        [FromQuery] int? idPropertyValue,
        [FromQuery] string? value,
        [FromQuery] string? idPropertyOrName)
    {
        try
        {
            var filter = new PropertyValueFilterDto
            {
                IdPropertyValue = idPropertyValue,
                Value = value,
                IdPropertyOrName = idPropertyOrName
            };

            var propertyValues = await _propertyValueService.GetFilteredAsync(filter);
            return Ok(new ApiResponse(propertyValues, "Property values retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPost("new-propertyValue")]
    public async Task<ActionResult> NewPropertyValue([FromBody] PropertyValueRequestDto propertyValueRequest)
    {
        try
        {
            var createdPropertyValue = await _propertyValueService.CreateAsync(propertyValueRequest);

            return CreatedAtAction(nameof(GetPropertyValues), new { idPropertyValue = createdPropertyValue.IdPropertyValue },
                new ApiResponse(createdPropertyValue, "Property value created successfully", 201));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit-propertyValue/{id}")]
    public async Task<ActionResult> UpdatePropertyValue(int id, [FromBody] PropertyValueUpdateDto propertyValueUpdate)
    {
        try
        {
            var updatedPropertyValue = await _propertyValueService.UpdateAsync(id, propertyValueUpdate);
            return Ok(new ApiResponse(updatedPropertyValue, "Property value updated successfully", 200));
        }
        catch (KeyNotFoundException keyEx)
        {
            return NotFound(new ApiResponse(keyEx.Message, 404));
        }
        catch (InvalidOperationException opEx)
        {
            return BadRequest(new ApiResponse(opEx.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("delete-propertyValue/{id}")]
    public async Task<ActionResult> DeletePropertyValue(int id)
    {
        try
        {
            var result = await _propertyValueService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Property value with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Property value deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("force-delete-propertyValue/{id}")]
    public async Task<ActionResult> ForceDeletePropertyValue(int id)
    {
        try
        {
            var result = await _propertyValueService.ForceDeleteAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Property value with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Property value force deleted successfully. Related records updated with generic value", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    // Warehouse Controller

    [HttpPost("new-warehouse")]
    public async Task<ActionResult> NewWarehouse([FromBody] WarehouseRequestDto warehouseRequest)
    {
        try
        {
            var createdWarehouse = await _warehouseService.CreateAsync(warehouseRequest);

            return CreatedAtAction(nameof(GetWarehouses), new { id = createdWarehouse.IdWarehouse },
                new ApiResponse(createdWarehouse, "Warehouse created successfully", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpGet("get-warehouses")]
    public async Task<ActionResult> GetWarehouses([FromQuery] int? id, [FromQuery] string? code)
    {
        try
        {
            if (id.HasValue)
            {
                var warehouse = await _warehouseService.GetByIdAsync(id.Value);
                if (warehouse == null)
                    return NotFound(new ApiResponse($"Warehouse with ID {id} not found", 404));

                return Ok(new ApiResponse(warehouse, "Warehouse retrieved successfully", 200));
            }

            if (!string.IsNullOrEmpty(code))
            {
                var warehouse = await _warehouseService.GetByCodeAsync(code);
                if (warehouse == null)
                    return NotFound(new ApiResponse($"Warehouse with code '{code}' not found", 404));

                return Ok(new ApiResponse(warehouse, "Warehouse retrieved successfully", 200));
            }

            var warehouses = await _warehouseService.GetAllAsync();
            return Ok(new ApiResponse(warehouses, "Warehouses retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit-warehouse/{id}")]
    public async Task<ActionResult> UpdateWarehouse(int id, [FromBody] WarehouseRequestDto warehouseRequest)
    {
        try
        {
            var updatedWarehouse = await _warehouseService.UpdateAsync(id, warehouseRequest);
            return Ok(new ApiResponse(updatedWarehouse, "Warehouse updated successfully", 200));
        }
        catch (KeyNotFoundException keyEx)
        {
            return NotFound(new ApiResponse(keyEx.Message, 404));
        }
        catch (InvalidOperationException opEx)
        {
            return BadRequest(new ApiResponse(opEx.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("delete-warehouse/{id}")]
    public async Task<ActionResult> DeleteWarehouse(int id)
    {
        try
        {
            var result = await _warehouseService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Warehouse with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Warehouse deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    // Supplier Controller

    [HttpPost("new-supplier")]
    public async Task<ActionResult> NewSupplier([FromBody] CreateSupplierDto supplierRequest)
    {
        try
        {
            var createdSupplier = await _supplierService.CreateSupplierAsync(supplierRequest);

            return CreatedAtAction(nameof(GetSuppliers), new { id = createdSupplier.IdSupplier },
                new ApiResponse(createdSupplier, "Supplier created successfully", 201));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpGet("get-suppliers")]
    public async Task<ActionResult> GetSuppliers()
    {
        try
        {
            var suppliers = await _supplierService.GetSuppliersBasicAsync();

            return Ok(new ApiResponse(suppliers, "Suppliers retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpGet("get-supplier/{id}")]
    public async Task<ActionResult> GetSupplier(int id)
    {
        try
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);

            if (supplier == null)
            {
                return NotFound(new ApiResponse($"Supplier with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(supplier, "Supplier retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit-supplier/{id}")]
    public async Task<ActionResult> EditSupplier(int id, [FromBody] UpdateSupplierDto supplierRequest)
    {
        try
        {
            var updatedSupplier = await _supplierService.UpdateSupplierAsync(id, supplierRequest);

            if (updatedSupplier == null)
            {
                return NotFound(new ApiResponse($"Supplier with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(updatedSupplier, "Supplier updated successfully", 200));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("delete-supplier/{id}")]
    public async Task<ActionResult> DeleteSupplier(int id)
    {
        try
        {
            var result = await _supplierService.DeleteSupplierAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Supplier with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Supplier deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

}