using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Taskedin.Domain.Common;

namespace Taskedin.Infrastructure.Filters;

/// <summary>
/// Runs the registered FluentValidation validator (if any) for each action
/// argument and short-circuits invalid requests with a 400 response using the
/// existing ApiResponse&lt;T&gt; format. Business rules that need database access
/// stay in the service layer.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null)
                continue;

            // Resolve the FluentValidation validator registered for this argument type, if any.
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (_serviceProvider.GetService(validatorType) is not IValidator validator)
                continue;

            var validationResult = await validator.ValidateAsync(
                new ValidationContext<object>(argument));

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                    .ToList();

                context.Result = new BadRequestObjectResult(
                    new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Data = errors
                    });

                return;
            }
        }

        await next();
    }
}
