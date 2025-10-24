using ELRakhawy.DAL.Services;
using ELRakhawy.EL.Models;
using Microsoft.AspNetCore.Mvc;

namespace ELRakhawy.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _authService.RegisterAsync(dto.FirstName, dto.LastName, dto.Email, dto.Password, dto.Role);
            return success ? Ok(new { Message = "User created successfully" }) : BadRequest(new { Message = "Email already exists" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _authService.LoginAsync(dto.Email, dto.Password);
            if (user == null)
                return Unauthorized(new { Message = "Invalid credentials" });

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.FullName,
                user.Email,
                user.Role
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var success = await _authService.ResetPasswordAsync(dto.UserId, dto.NewPassword, dto.RequesterRole);
                return success ? Ok(new { Message = "Password reset successfully" }) : NotFound(new { Message = "User not found" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
    public record RegisterDto(string FirstName, string LastName, string Email, string Password, UserRole Role);
    public record LoginDto(string Email, string Password);
    public record ResetPasswordDto(int UserId, string NewPassword, UserRole RequesterRole);
}
