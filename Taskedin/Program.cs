
using Taskedin.Infrastructure;
using Taskedin.Infrastructure.Middlewares;
using Taskedin.Service;
using Taskedin.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Taskedin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddServiceDependencies();
            builder.Services.AddPersistence(builder.Configuration);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Test database connection
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<TaskedinDbContext>();

                var canConnect = await db.Database.CanConnectAsync();

                Console.WriteLine(
                    $"UserManagementDb connected: {canConnect}");
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
