using System.Text;
using Taskedin.Infrastructure;
using Taskedin.Infrastructure.Filters;
using Taskedin.Infrastructure.Middlewares;
using Taskedin.Service;
using Taskedin.Service.Security;
using Taskedin.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Taskedin.Persistence.Context;

namespace Taskedin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers(options =>
            {
                // Validates request DTOs with FluentValidation before actions run.
                options.Filters.Add<ValidationFilter>();
            });
            builder.Services.AddScoped<ValidationFilter>();
            builder.Services.AddServiceDependencies();
            builder.Services.AddPersistence(builder.Configuration);

            // JWT Bearer authentication
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var signingKey = jwtSection["SigningKey"];

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Tokens are issued with short claim names (sub/name/email/role),
                    // so disable inbound mapping and state the claim types explicitly.
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSection["Issuer"] ?? "Taskedin",
                        ValidateAudience = true,
                        ValidAudience = jwtSection["Audience"] ?? "Taskedin",
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role,
                        // Left unset when the key is missing so EF Core's design-time
                        // host initialization cannot fail here; the startup check below
                        // fails fast before any request is served.
                        IssuerSigningKey = string.IsNullOrWhiteSpace(signingKey)
                            ? null
                            : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Taskedin API", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token: [space] <token>"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Enforced at startup only. EF Core's design-time host initialization
            // (dotnet ef migrations add) stops at builder.Build(), so generating
            // migrations does not require the signing key to be present.
            if (string.IsNullOrWhiteSpace(signingKey))
            {
                throw new InvalidOperationException(
                    "JWT signing key is not configured. " +
                    "Set 'Jwt:SigningKey' via user secrets or environment variables " +
                    "(e.g. 'dotnet user-secrets set \"Jwt:SigningKey\" <key>' in the Taskedin project).");
            }

            // Test database connection
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<TaskedinDbContext>();

                var canConnect = await db.Database.CanConnectAsync();

                Console.WriteLine(
                    $"UserManagementDb connected: {canConnect}");
            }

            // Global handler must run first so later failures return a standard 500 body.
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
