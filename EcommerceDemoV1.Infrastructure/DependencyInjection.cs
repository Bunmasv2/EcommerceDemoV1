using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EcommerceDemoV1.Infrastructure.ExternalServices;

namespace EcommerceDemoV1.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? Environment.GetEnvironmentVariable("DB_CONNECTION");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        // // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IPromotionRuleRepository, PromotionRuleRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();

        // // Services
        services.AddScoped<IPayOsService, PayOsService>();
        // services.AddScoped<IVnPayService, VnPayService>();

        // HttpClient for Ahamove
        services.AddHttpClient<IAhamoveService, AhamoveService>();

        // // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}