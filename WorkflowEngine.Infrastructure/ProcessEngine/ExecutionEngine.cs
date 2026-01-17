using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine.Infrastructure.ProcessEngine.Services;

namespace WorkflowEngine.Infrastructure.ProcessEngine;

public class ExecutionEngine
{
    private readonly Dictionary<string, string> _connectionStrings;
    private readonly IServiceProvider _serviceProvider; // ✅ Use IServiceProvider instead of ApplicationsService directly
    private ModuleCache _moduleCache;


    public ExecutionEngine(Dictionary<string, string> connectionStrings, IServiceProvider serviceProvider)
    {
        _connectionStrings = connectionStrings;
        _serviceProvider = serviceProvider;
        _moduleCache = new ModuleCache();
    }

    public ModuleCache Cache => _moduleCache;

    public async Task LoadApplicationsToEngineAsync(CancellationToken cancellationToken = default)
    {
        // Create a scope to get ApplicationsService (which needs scoped DbContext)
        using (var scope = _serviceProvider.CreateScope())
        {
            var applicationsService = scope.ServiceProvider.GetRequiredService<ApplicationsService>();

            // Clear existing cache before reloading
            _moduleCache.ClearAll();

            // Load applications into the engine's cache
            await applicationsService.LoadApplicationsIntoCache(_moduleCache, cancellationToken);
        }
    }
}
