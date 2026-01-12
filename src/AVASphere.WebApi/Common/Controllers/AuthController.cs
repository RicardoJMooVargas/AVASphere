using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.System.Interfaces;
using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AVASphere.WebApi.Common.Controllers
{
    [ApiController]
    [Route("api/common/[controller]")]
    [ApiExplorerSettings(GroupName = "Common")]
    [Tags("Common - Authentication")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTOs loginDtOs)
        {
            try
            {
                _logger.LogInformation("Solicitud de login recibida para usuario: {UserName}", loginDtOs.UserName);

                if (string.IsNullOrEmpty(loginDtOs.UserName) || string.IsNullOrEmpty(loginDtOs.Password))
                {
                    return BadRequest(new ApiResponse("Username and password are required", 400));
                }

                // 🔐 Usar el nuevo método de autenticación
                var authResult = await _userService.AuthenticateUserAsync(loginDtOs);
        
                if (!authResult.Success)
                {
                    _logger.LogWarning("Autenticación fallida para usuario: {UserName} - {Message}", 
                        loginDtOs.UserName, authResult.Message);
                    return Unauthorized(new ApiResponse(authResult.Message, 401));
                }

                // ✅ Usar GenerateToken con UserResponse
                var token = _tokenService.GenerateToken(authResult.User!);

                _logger.LogInformation("Login exitoso para usuario: {UserName}", loginDtOs.UserName);

                // ✅ RESPONSE ACTUALIZADO CON CONFIG SYS
                var loginData = new
                {
                    token = token,
                    user = new
                    {
                        id = authResult.User!.IdUsers,
                        userName = authResult.User.UserName,
                        name = authResult.User.Name,
                        lastName = authResult.User.LastName,
                        rol = authResult.User.RolName,
                        aux = authResult.User.Aux,
                        idConfigSys = authResult.User.IdConfigSys,
                        modules = authResult.User.Modules,
                        permissions = authResult.User.Permissions
                    },
                    configSys = authResult.ConfigSys
                };

                return Ok(new ApiResponse(loginData, "Authentication successful", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login para usuario: {UserName}", loginDtOs.UserName);
                return StatusCode(500, new ApiResponse("Internal server error", 500, new { error = ex.Message }));
            }
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <returns>Token validation result</returns>
        [HttpGet("validate-token")]
        public ActionResult ValidateToken()
        {
            // This endpoint will validate the JWT token through middleware
            var tokenData = new
            {
                user = HttpContext.User.Identity?.Name,
                isAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false
            };

            return Ok(new ApiResponse(tokenData, "Token is valid", 200));
        }
    }
}