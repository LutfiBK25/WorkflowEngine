using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Application.Session.Interfaces;

namespace WorkflowEngine.Infrastructure.Session.Workers;


public class SessionCleanupService : BackgroundService
{
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _sessionInActiveMaxAge = TimeSpan.FromHours(1);

    public SessionCleanupService(
       ISessionManager sessionManager,
       ILogger<SessionCleanupService> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Session cleanup service starting. Interval: {Interval}, Max Age: {MaxAge}",
            _cleanupInterval, _sessionInActiveMaxAge);

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
