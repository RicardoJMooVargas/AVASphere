using Microsoft.AspNetCore.Mvc;
using VYAACentralInforApi.Application.System.Interfaces;
using VYAACentralInforApi.Domain.System;

namespace VYAACentralInforApi.WebApi.System.Controllers
{
    [ApiController]
    [Route("api/system/[controller]")]
    [ApiExplorerSettings(GroupName = "System")]
    [Tags("System - Users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get all users from the database
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User details</returns>
        [HttpGet("by-username/{username}")]
        public async Task<ActionResult<Users>> GetUserByUserName(string username)
        {
            try
            {
                var user = await _userService.GetUserByUserNameAsync(username);
                
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}