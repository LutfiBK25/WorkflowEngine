

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine.Application.Session.Interfaces;
using WorkflowEngine.Infrastructure.ProcessEngine;
using WorkflowEngine.Infrastructure.ProcessEngine.Presistence;
using WorkflowEngine.Infrastructure.ProcessEngine.Services;
using WorkflowEngine.Infrastructure.ProcessEngine.Workers;
using WorkflowEngine.Infrastructure.Session;
using WorkflowEngine.Infrastructure.Session.Workers;

namespace WorkflowEngine.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // =============================================
        // DATABASE CONTEXT (SCOPED)
        // =============================================
        var repoConnectionString = configuration.GetConnectionString("RepositoryDB");
        services.AddDbContext<RepositoryDBContext>(options => options.UseNpgsql(repoConnectionString));

        // =============================================
        // CONNECTION STRINGS DICTIONARY (SINGLETON)
        // =============================================
        var connectionStrings = new Dictionary<string, string>();

        var wmsConnection = configuration.GetConnectionString("WMS");
        var engineConnection = configuration.GetConnectionString("Engine");

        if (!string.IsNullOrEmpty(wmsConnection))
        {
            connectionStrings["WMS"] = wmsConnection;
            connectionStrings["DEFAULT"] = wmsConnection;
        }

        if (!string.IsNullOrEmpty(engineConnection))
        {
            connectionStrings["ENGINE"] = engineConnection;
        }


        services.AddSingleton(connectionStrings);

        // =============================================
        // SERVICES
        // =============================================

        // Application Service
        services.AddScoped<ApplicationsService>();

        // Execution Engine
        services.AddSingleton<ExecutionEngine>();

        // Session Management
        services.AddSingleton<ISessionStore, InMemorySessionStore>();
        services.AddSingleton<ISessionManager, SessionManager>();

        // Background cleanup service
        services.AddHostedService<SessionCleanupService>();

        // Background Worker
        services.AddHostedService<WorkflowEngineWorker>();
    }
}
