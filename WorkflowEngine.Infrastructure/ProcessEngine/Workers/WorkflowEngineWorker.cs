
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Infrastructure.ProcessEngine.Presistence;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Workers
{
    public class WorkflowEngineWorker : BackgroundService
    {
        private readonly ILogger<WorkflowEngineWorker> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ExecutionEngine _executionEngine;


        public WorkflowEngineWorker(
            ILogger<WorkflowEngineWorker> logger,
            IHostApplicationLifetime lifetime,
            ExecutionEngine executionEngine)
        {
            _logger = logger;
            _lifetime = lifetime;
            _executionEngine = executionEngine;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("║         QULRON ENGINE SERVICE - STARTUP         ║");
                _logger.LogInformation("Workflow Engine Worker starting at: {StartTime}", DateTimeOffset.Now);

                // ✅ Load applications into the engine's cache
                await _executionEngine.LoadApplicationsToEngineAsync(stoppingToken);

                // We are here in code execution (we can create a sample session here)

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "ENGINE STARTUP FAILED");
                _lifetime.StopApplication();
                throw; // Let the host handle it
            }
            return;
        }
    }
}
