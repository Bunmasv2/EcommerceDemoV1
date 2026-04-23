using EcommerceDemoV1.Application.Features.Auth.Commands.Register;
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

        services.AddValidatorsFromAssembly(typeof(RegisterUserCommandValidator).Assembly);

        return services;
    }
}