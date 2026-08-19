using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrickShowcase.Infrastructure.Data;
using BrickShowcase.Application.DTOs;
using System.Security.Cryptography;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BrickShowcase.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BrickDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(BrickDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var username = request.Username?.Trim() ?? "";
        var password = request.Password?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest(new LoginResponseDto(false, "", "Vui lòng nhập tài khoản và mật khẩu"));
        }

        var user = await _db.AdminUser.FirstOrDefaultAsync(u => u.Username.Trim() == username);

        if (user != null && VerifyPassword(password, user.PasswordHash))
        {
            var token = GenerateJwtToken(user.Username);
            return Ok(new LoginResponseDto(true, token, "Đăng nhập thành công"));
        }

        return BadRequest(new LoginResponseDto(false, "", "Tài khoản hoặc mật khẩu không chính xác"));
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<LoginResponseDto>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new LoginResponseDto(false, "", "Vui lòng nhập đầy đủ thông tin tài khoản, mật khẩu cũ và mật khẩu mới"));
        }

        var user = await _db.AdminUser.FirstOrDefaultAsync(u => u.Username.Trim() == request.Username.Trim());

        if (user == null || !VerifyPassword(request.OldPassword.Trim(), user.PasswordHash))
        {
            return BadRequest(new LoginResponseDto(false, "", "Tài khoản hoặc mật khẩu hiện tại không chính xác"));
        }

        user.PasswordHash = HashPassword(request.NewPassword.Trim());
        await _db.SaveChangesAsync();

        return Ok(new LoginResponseDto(true, "", "Đổi mật khẩu thành công. Vui lòng đăng nhập lại bằng mật khẩu mới!"));
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSecretKey = _config["Jwt:SecretKey"] ?? "SuperSecretKeyForBrickShowcaseApiProductionEnvironment2026!";
        var jwtIssuer = _config["Jwt:Issuer"] ?? "BrickShowcaseApi";
        var jwtAudience = _config["Jwt:Audience"] ?? "BrickShowcaseClient";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool VerifyPassword(string inputPassword, string storedHash)
    {
        var cleanInput = inputPassword.Trim();
        var cleanStored = storedHash.Trim();

        if (cleanInput.Equals(cleanStored, StringComparison.OrdinalIgnoreCase)) return true;
        
        var hashHex = HashPassword(cleanInput);
        return hashHex.Equals(cleanStored, StringComparison.OrdinalIgnoreCase);
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

