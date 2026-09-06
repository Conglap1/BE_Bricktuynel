using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
            // Auto upgrade legacy SHA256 hash to PBKDF2 with salt
            if (!user.PasswordHash.StartsWith("pbkdf2:"))
            {
                user.PasswordHash = HashPassword(password);
                await _db.SaveChangesAsync();
            }

            var token = GenerateJwtToken(user.Username);
            return Ok(new LoginResponseDto(true, token, "Đăng nhập thành công"));
        }

        return BadRequest(new LoginResponseDto(false, "", "Tài khoản hoặc mật khẩu không chính xác"));
    }

    [HttpPost("change-password")]
    [Authorize]
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

    public static bool VerifyPassword(string inputPassword, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(inputPassword) || string.IsNullOrWhiteSpace(storedHash)) return false;

        var cleanInput = inputPassword.Trim();
        var cleanStored = storedHash.Trim();

        // New PBKDF2 format: pbkdf2:100000:<salt_hex>:<hash_hex>
        if (cleanStored.StartsWith("pbkdf2:"))
        {
            var parts = cleanStored.Split(':');
            if (parts.Length != 4) return false;

            if (!int.TryParse(parts[1], out int iterations)) return false;
            var salt = Convert.FromHexString(parts[2]);
            var expectedHash = Convert.FromHexString(parts[3]);

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(cleanInput, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }

        // Legacy SHA256 check for backwards compatibility
        using var sha = SHA256.Create();
        var inputHash = sha.ComputeHash(Encoding.UTF8.GetBytes(cleanInput));
        try
        {
            var storedBytes = Convert.FromHexString(cleanStored);
            return CryptographicOperations.FixedTimeEquals(inputHash, storedBytes);
        }
        catch
        {
            return false;
        }
    }

    public static string HashPassword(string password)
    {
        const int iterations = 100_000;
        const int saltSize = 16;
        const int hashSize = 32;

        var salt = RandomNumberGenerator.GetBytes(saltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password.Trim(), salt, iterations, HashAlgorithmName.SHA256, hashSize);

        return $"pbkdf2:{iterations}:{Convert.ToHexString(salt)}:{Convert.ToHexString(hash)}";
    }
}


