using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BrickShowcase.Infrastructure;
using BrickShowcase.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52_428_800; // 50MB
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Register Infrastructure services (DbContext, FileStorage, etc.)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "SuperSecretKeyForBrickShowcaseApiProductionEnvironment2026!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BrickShowcaseApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "BrickShowcaseClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure CORS for Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Seed Initial Database Data
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BrickDbContext>();
        await DbInitializer.SeedAsync(db);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[DB INIT ERROR]: {ex.Message}");
}

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseStaticFiles(); // Serves wwwroot/uploads

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

