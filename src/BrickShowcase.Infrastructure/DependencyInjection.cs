using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BrickShowcase.Infrastructure.Data;
using BrickShowcase.Infrastructure.Services;
using BrickShowcase.Application.Interfaces;

namespace BrickShowcase.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=BrickCompanyDB;User Id=sa;Password=Phanconglap1@;TrustServerCertificate=True;";

        services.AddDbContext<BrickDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}
