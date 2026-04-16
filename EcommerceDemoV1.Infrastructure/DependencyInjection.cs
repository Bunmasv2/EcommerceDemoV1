using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace EcommerceDemoV1.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? Environment.GetEnvironmentVariable("DB_CONNECTION");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        // // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtRepository, JwtRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        // services.AddScoped<ICartRepository, CartRepository>();
        // services.AddScoped<IOrderRepository, OrderRepository>();
        // services.AddScoped<ICouponRepository, CouponRepository>();
        // services.AddScoped<IReviewRepository, ReviewRepository>();

        // // Services
        // services.AddScoped<IVnPayService, VnPayService>();

        // // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}