using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Taskedin.Domain.Entities;
using Taskedin.Service.DTOs;
using Taskedin.Service.Interfaces;
using Taskedin.Service.Services;
using Taskedin.Service.Validators;

namespace Taskedin.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddServiceDependencies(
        this IServiceCollection services)
    {
        services.AddScoped<IDepartmentService, DepartmentService>();
        // Periodic background monitor that verifies database connectivity.
        services.AddHostedService<SampleBackgroundService>();

        // Authentication / authorization services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        // FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}