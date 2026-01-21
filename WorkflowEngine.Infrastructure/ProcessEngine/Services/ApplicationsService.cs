

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;
using WorkflowEngine.Infrastructure.ProcessEngine.Presistence;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Services;

public class ApplicationsService
{
    private readonly RepositoryDBContext _repositoryDBContext;
    private readonly ILogger<ApplicationsService> _logger;

    public ApplicationsService(RepositoryDBContext repositoryDBContext, ILogger<ApplicationsService> logger)
    {
        _repositoryDBContext = repositoryDBContext; 
        _logger = logger;
    }

    // Loads Applications into ModuleCache
    public async Task<ModuleCache> LoadApplicationsIntoCache(ModuleCache moduleCache, CancellationToken stoppingToken)
    {
        // Load only applications that should activate on start
        var applications = await _repositoryDBContext.Applications
            .Where(a => a.ActivateOnStart) // Filter by ActivateOnStart
            .ToListAsync(stoppingToken);

        if(applications.Count == 0)
        {
            _logger.LogWarning("There is no applications to be loaded on engine start");
            return moduleCache;
        }
        _logger.LogInformation("Found {Count} applications with ActivateOnStart=true", applications.Count);

        foreach (var app in applications)
        {
            // Check if cancellation was requested before processing each app
            stoppingToken.ThrowIfCancellationRequested(); // Respects shutdown between apps

            var allModules = new List<Module>();

            // Load each module type separately (TPC requires this)
            var processModules = await _repositoryDBContext.ProcessModules
                .Include(pm => pm.Details) // Load the steps!
                .Where(m => m.ApplicationId == app.Id)
                .ToListAsync(stoppingToken);

            var dbActionModules = await _repositoryDBContext.DatabaseActionModules
                .Where(m => m.ApplicationId == app.Id)
                .ToListAsync(stoppingToken);

            var dialogModules = await _repositoryDBContext.DialogActionModules
                .Where(m => m.ApplicationId == app.Id)
                .ToListAsync(stoppingToken);

            var fieldModules = await _repositoryDBContext.FieldModules
                .Where(m => m.ApplicationId == app.Id)
                .ToListAsync(stoppingToken);

            var cmpActionModules = await _repositoryDBContext.CompareActionsModules
                .Where(m => m.ApplicationId == app.Id)
                .ToListAsync(stoppingToken);

            var calcActionModules = await _repositoryDBContext.CalculateActionsModules
                .Include(cm => cm.Details)
                .Where(m => m.ApplicationId == app.Id)
                .ToListAsync(stoppingToken);

            var listModules = await _repositoryDBContext.ListModules
                .Where(m => m.ApplicationId == app.Id)
                .ToListAsync(stoppingToken);

            // Combine all modules
            allModules.AddRange(processModules);
            allModules.AddRange(dbActionModules);
            allModules.AddRange(dialogModules);
            allModules.AddRange(fieldModules);
            allModules.AddRange(cmpActionModules);
            allModules.AddRange(calcActionModules);
            allModules.AddRange(listModules);

            // Load into cache
            moduleCache.LoadApplicationModules(app.Id, allModules);


            _logger.LogInformation(
                "Loaded {ProcessCount} process, {DbCount} database, {DialogCount} dialog," +
                " {FieldCount} field, {CompareCount} compare, {CalcCount} calculate, {ListCount} list modules for application '{AppName}'",
                processModules.Count, dbActionModules.Count, dialogModules.Count,
                fieldModules.Count, cmpActionModules.Count, calcActionModules.Count,
                listModules.Count, app.Name);
        }
        _logger.LogInformation("Successfully loaded all {Count} applications into cache", applications.Count);

        return moduleCache;

    }
}
