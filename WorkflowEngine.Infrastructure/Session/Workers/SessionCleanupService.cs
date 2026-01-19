using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkflowEngine.Infrastructure.Session.Workers;


public class SessionCleanupService : BackgroundService
{
    private readonly SessionManager _sessionManager;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _sessionInActiveMaxAge = TimeSpan.FromHours(1);

    public SessionCleanupService(
       SessionManager sessionManager,
       ILogger<SessionCleanupService> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session cleanup service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                _logger.LogInformation("Running session cleanup");
                await _sessionManager.CleanupExpiredSessionsAsync(_sessionInActiveMaxAge, stoppingToken);
                _logger.LogInformation("Session cleanup completed");
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session cleanup");
            }
        }

        _logger.LogInformation("Session cleanup service stopped");
    }
}
