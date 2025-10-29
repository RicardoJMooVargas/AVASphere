using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.System.Interfaces;
using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AVASphere.WebApi.Common.Controllers
{
    [ApiController]
    [Route("api/system/[controller]")]
    [ApiExplorerSettings(GroupName = "System")]
    [Tags("System - Authentication")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        // ✅ CORREGIR: Agregar ILogger al constructor
        public AuthController(IUserService userService, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger; // ✅ Inicializar el logger
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation("Solicitud de login recibida para usuario: {UserName}", loginRequest.UserName);

                if (string.IsNullOrEmpty(loginRequest.UserName) || string.IsNullOrEmpty(loginRequest.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }

                // 🔐 Usar el nuevo método de autenticación
                var authResult = await _userService.AuthenticateUserAsync(loginRequest);
        
                if (!authResult.Success)
                {
                    _logger.LogWarning("Autenticación fallida para usuario: {UserName} - {Message}", 
                        loginRequest.UserName, authResult.Message);
                    return Unauthorized(new { message = authResult.Message });
                }

                // ✅ Usar GenerateToken con UserResponse
                var token = _tokenService.GenerateToken(authResult.User!);

                _logger.LogInformation("Login exitoso para usuario: {UserName}", loginRequest.UserName);

                return Ok(new
                {
                    message = "Authentication successful",
                    token = token,
                    user = new
                    {
                        id = authResult.User!.IdUsers,
                        userName = authResult.User.UserName,
                        name = authResult.User.Name,
                        lastName = authResult.User.LastName,
                        rol = authResult.User.RolName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login para usuario: {UserName}", loginRequest.UserName);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
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
            return Ok(new
            {
                message = "Token is valid",
                user = HttpContext.User.Identity?.Name,
                timestamp = DateTime.UtcNow
            });
        }

        // ❌ ELIMINAR este método - ya no es necesario porque usas EncryptionService
        // private static bool VerifyPassword(string inputPassword, string hashedPassword)
        // {
        //     if (string.IsNullOrEmpty(hashedPassword))
        //         return false;
        //
        //     using var sha256 = SHA256.Create();
        //     var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
        //     var hashedInput = Convert.ToBase64String(hashedBytes);
        //     
        //     return hashedInput == hashedPassword;
        // }
    }
}