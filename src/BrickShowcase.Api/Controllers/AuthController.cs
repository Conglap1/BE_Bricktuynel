using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;

    public AuthController(BrickDbContext db, IConfiguration config, IMemoryCache cache)
    {
        _db = db;
        _config = config;
        _cache = cache;
    }

    [HttpPost("login")]
    [EnableRateLimiting("LoginRateLimit")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var username = request.Username?.Trim() ?? "";
        var password = request.Password?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest(new LoginResponseDto(false, "", "Vui lòng nhập tài khoản và mật khẩu"));
        }

        var normalizedUser = username.ToLowerInvariant();
        var lockKey = $"lockout_{normalizedUser}";
        var failKey = $"fail_attempts_{normalizedUser}";

        // Check if account is currently locked out
        if (_cache.TryGetValue(lockKey, out DateTime lockExpires))
        {
            var remainingMinutes = Math.Max(1, (int)Math.Ceiling((lockExpires - DateTime.UtcNow).TotalMinutes));
            return BadRequest(new LoginResponseDto(false, "", $"Tài khoản tạm thời bị khóa do nhập sai mật khẩu quá 5 lần. Vui lòng thử lại sau {remainingMinutes} phút."));
        }

        var user = await _db.AdminUser.FirstOrDefaultAsync(u => u.Username.Trim().ToLower() == normalizedUser);

        if (user != null && VerifyPassword(password, user.PasswordHash))
        {
            // Reset fail attempts on successful login
            _cache.Remove(failKey);
            _cache.Remove(lockKey);

            // Auto upgrade legacy SHA256 hash to PBKDF2 with salt
            if (!user.PasswordHash.StartsWith("pbkdf2:"))
            {
                user.PasswordHash = HashPassword(password);
                await _db.SaveChangesAsync();
            }

            var token = GenerateJwtToken(user.Username);
            return Ok(new LoginResponseDto(true, token, "Đăng nhập thành công"));
        }

        // Increment failed attempts
        var attempts = _cache.GetOrCreate(failKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            return 0;
        }) + 1;

        _cache.Set(failKey, attempts, TimeSpan.FromMinutes(15));

        if (attempts >= 5)
        {
            _cache.Set(lockKey, DateTime.UtcNow.AddMinutes(15), TimeSpan.FromMinutes(15));
            _cache.Remove(failKey);
            return BadRequest(new LoginResponseDto(false, "", "Tài khoản tạm thời bị khóa 15 phút do nhập sai mật khẩu quá 5 lần liên tiếp."));
        }

        var remainingAttempts = 5 - attempts;
        return BadRequest(new LoginResponseDto(false, "", $"Tài khoản hoặc mật khẩu không chính xác. Bạn còn {remainingAttempts} lần thử trước khi bị khóa tạm thời."));
    }

    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("LoginRateLimit")]
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
            expires: DateTime.UtcNow.AddHours(8),
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


