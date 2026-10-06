using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Taskedin.Service.Interfaces;
using Taskedin.Service.Services;

namespace Taskedin.External;

public static class DependencyInjection
{
    public static IServiceCollection addServices
        (
        this IServiceCollection services)
    {
        services.AddSingleton<IDepartmentService, DepartmentService>();

        return services;
    }
}