using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Taskedin.Service.Services;

// Background monitor that logs database connectivity health every few minutes.
public class SampleBackgroundService : BackgroundService
{
    private readonly ILogger<SampleBackgroundService> _logger;
    private readonly string _connectionString;

    public SampleBackgroundService(
        ILogger<SampleBackgroundService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Database connection string was not found.");
    }

    // Checks the database connection periodically.
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database health monitor started.");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        try
        {
            do
            {
                await CheckDatabaseConnectionAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Database health monitor stopped.");
        }
    }

    // Opens a connection to verify that the database is available.
    private async Task CheckDatabaseConnectionAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            _logger.LogInformation("Database connection is healthy.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection check failed.");
        }
    }
}