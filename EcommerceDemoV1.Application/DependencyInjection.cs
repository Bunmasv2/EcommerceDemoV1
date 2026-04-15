using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceDemoV1.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

        // services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

        return services;
    }
}