using Microsoft.AspNetCore.Mvc;
using VYAACentralInforApi.ApplicationCore.System.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace VYAACentralInforApi.WebApi.System.Controllers
{
    [ApiController]
    [Route("api/system/[controller]")]
    [ApiExplorerSettings(GroupName = "System")]
    [Tags("System - Authentication")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Authenticate user and generate JWT token
        /// </summary>
        /// <param name="loginRequest">User credentials</param>
        /// <returns>JWT token if authentication successful</returns>
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(loginRequest.UserName) || string.IsNullOrEmpty(loginRequest.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }

                var user = await _userService.GetUserByUserNameAsync(loginRequest.UserName);
                
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Verificar contraseña hasheada
                if (!VerifyPassword(loginRequest.Password, user.HashPassword))
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                if (!user.Status)
                {
                    return Unauthorized(new { message = "User account is disabled" });
                }

                var token = _tokenService.GenerateToken(user);

                return Ok(new
                {
                    message = "Authentication successful",
                    token = token,
                    user = new
                    {
                        id = user.IdUsers,
                        userName = user.UserName,
                        name = user.Name,
                        lastName = user.LastName,
                        rol = user.Rol
                    }
                });
            }
            catch (Exception ex)
            {
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

        private static bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            var hashedInput = Convert.ToBase64String(hashedBytes);
            
            return hashedInput == hashedPassword;
        }
    }

    public class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
