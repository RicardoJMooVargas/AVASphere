﻿﻿using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Common.Interfaces;
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
    private readonly ILogger<CatalogsController> _logger;

    public CatalogsController(IAreaService areaService, IProjectCategoryService projectCategoryService, IPropertyService propertyService,
        IPropertyValueService propertyValueService, ISupplierService supplierService, ILogger<CatalogsController> logger)
    {
        _areaService = areaService;
        _logger = logger;
        _propertyService = propertyService;
        _propertyValueService = propertyValueService;
        _projectCategoryService = projectCategoryService;
        _supplierService = supplierService;
        
    }

    //Area Controller

    [HttpPost("new-area")]
    public async Task<ActionResult> NewArea([FromBody] AreaRequestDto areaRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new area with name: {AreaName}", areaRequest.Name);
            var createdArea = await _areaService.CreateAsync(areaRequest);

            return CreatedAtAction(nameof(GetAreas), new { id = createdArea.IdArea },
                new ApiResponse(createdArea, "Area created successfully", 201));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating area: {AreaName}", areaRequest.Name);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating area with name: {AreaName}", areaRequest.Name);
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
                _logger.LogInformation("Retrieving area with ID: {AreaId}", id);
                var area = await _areaService.GetByIdAsync(id.Value);
                if (area == null)
                    return NotFound(new ApiResponse($"Area with ID {id} not found", 404));

                return Ok(new ApiResponse(area, "Area retrieved successfully", 200));
            }

            if (!string.IsNullOrEmpty(name))
            {
                _logger.LogInformation("Retrieving area with name: {AreaName}", name);
                var area = await _areaService.GetByNameAsync(name);
                if (area == null)
                    return NotFound(new ApiResponse($"Area with name '{name}' not found", 404));

                return Ok(new ApiResponse(area, "Area retrieved successfully", 200));
            }

            _logger.LogInformation("Retrieving all areas");
            var areas = await _areaService.GetAllAsync();
            return Ok(new ApiResponse(areas, "Areas retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving areas");
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit-areas/{id}")]
    public async Task<ActionResult> UpdateArea(int id, [FromBody] AreaRequestDto areaRequest)
    {
        try
        {
            _logger.LogInformation("Updating area with ID: {AreaId}", id);
            var updatedArea = await _areaService.UpdateAsync(id, areaRequest);
            return Ok(new ApiResponse(updatedArea, "Area updated successfully", 200));
        }
        catch (KeyNotFoundException keyEx)
        {
            _logger.LogWarning(keyEx, "Area not found for update: {AreaId}", id);
            return NotFound(new ApiResponse(keyEx.Message, 404));
        }
        catch (InvalidOperationException opEx)
        {
            _logger.LogWarning(opEx, "Business rule violation while updating area: {AreaId}", id);
            return BadRequest(new ApiResponse(opEx.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating area with ID: {AreaId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("delete-areas/{id}")]
    public async Task<ActionResult> DeleteArea(int id)
    {
        try
        {
            _logger.LogInformation("Deleting area with ID: {AreaId}", id);
            var result = await _areaService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Area with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Area deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while deleting area: {AreaId}", id);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting area with ID: {AreaId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }


    //Project Category Controller

    [HttpPost("new-projectCategory")]
    public async Task<ActionResult> NewProjectCategory([FromBody] ProjectCategoryRequestDto projectCategoryRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new category with name: {ProjectCategoryName}",
                projectCategoryRequest.Name);
            var createdProjectCategory = await _projectCategoryService.CreateAsync(projectCategoryRequest);

            return CreatedAtAction(nameof(GetProjectCategories), new { id = createdProjectCategory.IdProjectCategory },
                new ApiResponse(createdProjectCategory, "Project Category created successfully", 201));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating category: {ProjectCategoryName}",
                projectCategoryRequest.Name);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating category with name: {ProjectCategoryName}",
                projectCategoryRequest.Name);
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
                _logger.LogInformation("Retrieving project category with ID: {ProjectCategoryId}", id);
                var projectCategory = await _projectCategoryService.GetByIdAsync(id.Value);
                if (projectCategory == null)
                    return NotFound(new ApiResponse($"Project category with ID {id} not found", 404));

                return Ok(new ApiResponse(projectCategory, "Project category retrieved successfully", 200));
            }

            if (!string.IsNullOrEmpty(name))
            {
                _logger.LogInformation("Retrieving project category with name: {ProjectCategoryName}", name);
                var projectCategory = await _projectCategoryService.GetByNameAsync(name);
                if (projectCategory == null)
                    return NotFound(new ApiResponse($"Project category with name '{name}' not found", 404));

                return Ok(new ApiResponse(projectCategory, "Project category retrieved successfully", 200));
            }

            _logger.LogInformation("Retrieving all project categories");
            var projectCategories = await _projectCategoryService.GetAllAsync();
            return Ok(new ApiResponse(projectCategories, "Project categories retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project categories");
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit-projectCategory/{id}")]
    public async Task<ActionResult> UpdateProjectCategory(int id,
        [FromBody] ProjectCategoryRequestDto projectCategoryRequest)
    {
        try
        {
            _logger.LogInformation("Updating project category with ID: {ProjectCategoryId}", id);
            var updatedProjectCategory = await _projectCategoryService.UpdateAsync(id, projectCategoryRequest);
            return Ok(new ApiResponse(updatedProjectCategory, "Project Category updated successfully", 200));
        }
        catch (KeyNotFoundException keyEx)
        {
            _logger.LogWarning(keyEx, "Project Category not found for update: {ProyectCategoryId}", id);
            return NotFound(new ApiResponse(keyEx.Message, 404));
        }
        catch (InvalidOperationException opEx)
        {
            _logger.LogWarning(opEx, "Business rule violation while updating project category: {ProjectCategoryId}",
                id);
            return BadRequest(new ApiResponse(opEx.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project category with ID: {ProjectCategoryId}", id);
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
              _logger.LogInformation("Creating a new property with name: {PropertyName}",
                  propertyRequest.Name);
              var createdProperty = await _propertyService.CreateAsync(propertyRequest);

              return CreatedAtAction(nameof(GetProperties), new { id = createdProperty.IdProperty },
                  new ApiResponse(createdProperty, "Property created successfully", 201));
          }
          catch (InvalidOperationException ex)
          {
              _logger.LogWarning(ex, "Business rule violation while creating property: {PropertyName}",
                  propertyRequest.Name);
              return BadRequest(new ApiResponse(ex.Message, 400));
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, "Error occurred while creating property with name: {PropertyName}",
                  propertyRequest.Name);
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
                  _logger.LogInformation("Retrieving property with ID: {PropertyId}", id);
                  var property = await _propertyService.GetByIdAsync(id.Value);
                  if (property == null)
                      return NotFound(new ApiResponse($"Property with ID {id} not found", 404));

                  return Ok(new ApiResponse(property, "Property retrieved successfully", 200));
              }

              if (!string.IsNullOrEmpty(name))
              {
                  _logger.LogInformation("Retrieving property with name: {PropertyName}", name);
                  var property = await _propertyService.GetByNameAsync(name);
                  if (property == null)
                      return NotFound(new ApiResponse($"Property with name '{name}' not found", 404));

                  return Ok(new ApiResponse(property, "Property retrieved successfully", 200));
              }

              _logger.LogInformation("Retrieving all properties");
              var properties = await _propertyService.GetAllAsync();
              return Ok(new ApiResponse(properties, "Property retrieved successfully", 200));
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, "Error retrieving properties");
              return StatusCode(500, new ApiResponse("Internal server error", 500));
          }
      }
      
      [HttpPut("edit-property/{id}")]
      public async Task<ActionResult> UpdateProperty(int id,
          [FromBody] PropertyRequestDto propertyRequest)
      {
          try
          {
              _logger.LogInformation("Updating property with ID: {PropertyId}", id);
              var updatedProperty = await _propertyService.UpdateAsync(id, propertyRequest);
              return Ok(new ApiResponse(updatedProperty, "Property updated successfully", 200));
          }
          catch (KeyNotFoundException keyEx)
          {
              _logger.LogWarning(keyEx, "Property not found for update: {PropertyId}", id);
              return NotFound(new ApiResponse(keyEx.Message, 404));
          }
          catch (InvalidOperationException opEx)
          {
              _logger.LogWarning(opEx, "Business rule violation while updating property: {PropertyId}",
                  id);
              return BadRequest(new ApiResponse(opEx.Message, 400));
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, "Error updating property with ID: {PropertyId}", id);
              return StatusCode(500, new ApiResponse("Internal server error", 500));
          }
      }
      
      [HttpDelete("delete-property/{id}")]
   public async Task<ActionResult> DeleteProperty(int id)
   {
       try
       {
           _logger.LogInformation("Deleting property with ID: {PropertyId}", id);
           var result = await _propertyService.DeleteAsync(id);

           if (!result)
           {
               return NotFound(new ApiResponse($"Property with ID {id} not found", 404));
           }

           return Ok(new ApiResponse(null, "Property deleted successfully", 200));
       }
       catch (InvalidOperationException ex)
       {
           _logger.LogWarning(ex, "Business rule violation while deleting property: {PropertyId}", id);
           return BadRequest(new ApiResponse(ex.Message, 400));
       }
       catch (Exception ex)
       {
           _logger.LogError(ex, "Error deleting property with ID: {PropertyId}", id);
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

            _logger.LogInformation("Retrieving property values with filters: IdPropertyValue={IdPropertyValue}, Value={Value}, IdPropertyOrName={IdPropertyOrName}", 
                idPropertyValue, value, idPropertyOrName);

            var propertyValues = await _propertyValueService.GetFilteredAsync(filter);
            return Ok(new ApiResponse(propertyValues, "Property values retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving property values");
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPost("new-propertyValue")]
    public async Task<ActionResult> NewPropertyValue([FromBody] PropertyValueRequestDto propertyValueRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new property value with value: {Value} for property ID: {IdProperty}", 
                propertyValueRequest.Value, propertyValueRequest.IdProperty);
            
            var createdPropertyValue = await _propertyValueService.CreateAsync(propertyValueRequest);

            return CreatedAtAction(nameof(GetPropertyValues), new { idPropertyValue = createdPropertyValue.IdPropertyValue },
                new ApiResponse(createdPropertyValue, "Property value created successfully", 201));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Property not found while creating property value");
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating property value: {Value}", 
                propertyValueRequest.Value);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating property value with value: {Value}", 
                propertyValueRequest.Value);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit-propertyValue/{id}")]
    public async Task<ActionResult> UpdatePropertyValue(int id, [FromBody] PropertyValueUpdateDto propertyValueUpdate)
    {
        try
        {
            _logger.LogInformation("Updating property value with ID: {PropertyValueId}", id);
            var updatedPropertyValue = await _propertyValueService.UpdateAsync(id, propertyValueUpdate);
            return Ok(new ApiResponse(updatedPropertyValue, "Property value updated successfully", 200));
        }
        catch (KeyNotFoundException keyEx)
        {
            _logger.LogWarning(keyEx, "Property value or property not found for update: {PropertyValueId}", id);
            return NotFound(new ApiResponse(keyEx.Message, 404));
        }
        catch (InvalidOperationException opEx)
        {
            _logger.LogWarning(opEx, "Business rule violation while updating property value: {PropertyValueId}", id);
            return BadRequest(new ApiResponse(opEx.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating property value with ID: {PropertyValueId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("delete-propertyValue/{id}")]
    public async Task<ActionResult> DeletePropertyValue(int id)
    {
        try
        {
            _logger.LogInformation("Deleting property value with ID: {PropertyValueId}", id);
            var result = await _propertyValueService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Property value with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Property value deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while deleting property value: {PropertyValueId}", id);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting property value with ID: {PropertyValueId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("force-delete-propertyValue/{id}")]
    public async Task<ActionResult> ForceDeletePropertyValue(int id)
    {
        try
        {
            _logger.LogInformation("Force deleting property value with ID: {PropertyValueId}", id);
            var result = await _propertyValueService.ForceDeleteAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Property value with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Property value force deleted successfully. Related records updated with generic value", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force deleting property value with ID: {PropertyValueId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    // Supplier Controller

    [HttpPost("new-supplier")]
    public async Task<ActionResult> NewSupplier([FromBody] CreateSupplierDto supplierRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new supplier with name: {SupplierName}", supplierRequest.Name);
            var createdSupplier = await _supplierService.CreateSupplierAsync(supplierRequest);

            return CreatedAtAction(nameof(GetSuppliers), new { id = createdSupplier.IdSupplier },
                new ApiResponse(createdSupplier, "Supplier created successfully", 201));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating supplier: {SupplierName}", supplierRequest.Name);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier with name: {SupplierName}", supplierRequest.Name);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpGet("get-suppliers")]
    public async Task<ActionResult> GetSuppliers()
    {
        try
        {
            _logger.LogInformation("Retrieving all suppliers");
            var suppliers = await _supplierService.GetSuppliersBasicAsync();

            return Ok(new ApiResponse(suppliers, "Suppliers retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers");
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpGet("get-supplier/{id}")]
    public async Task<ActionResult> GetSupplier(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving supplier with ID: {SupplierId}", id);
            var supplier = await _supplierService.GetSupplierByIdAsync(id);

            if (supplier == null)
            {
                return NotFound(new ApiResponse($"Supplier with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(supplier, "Supplier retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier with ID: {SupplierId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit-supplier/{id}")]
    public async Task<ActionResult> EditSupplier(int id, [FromBody] UpdateSupplierDto supplierRequest)
    {
        try
        {
            _logger.LogInformation("Editing supplier with ID: {SupplierId}", id);
            var updatedSupplier = await _supplierService.UpdateSupplierAsync(id, supplierRequest);

            if (updatedSupplier == null)
            {
                return NotFound(new ApiResponse($"Supplier with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(updatedSupplier, "Supplier updated successfully", 200));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while editing supplier with ID: {SupplierId}", id);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing supplier with ID: {SupplierId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("delete-supplier/{id}")]
    public async Task<ActionResult> DeleteSupplier(int id)
    {
        try
        {
            _logger.LogInformation("Deleting supplier with ID: {SupplierId}", id);
            var result = await _supplierService.DeleteSupplierAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Supplier with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Supplier deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete supplier with ID: {SupplierId}", id);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier with ID: {SupplierId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }
    
}