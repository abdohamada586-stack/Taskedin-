using Microsoft.Extensions.DependencyInjection;
using Taskedin.Service.Interfaces;
using Taskedin.Service.Services;

namespace Taskedin.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddServiceDependencies(
        this IServiceCollection services)
    {
        services.AddSingleton<IDepartmentService, DepartmentService>();

        return services;
    }
}