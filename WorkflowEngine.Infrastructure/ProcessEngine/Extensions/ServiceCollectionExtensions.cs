

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine.Infrastructure.ProcessEngine.Presistence;
using WorkflowEngine.Infrastructure.ProcessEngine.Services;
using WorkflowEngine.Infrastructure.ProcessEngine.Workers;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Extensions;

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
        services.AddScoped<ApplicationsService>();
        services.AddSingleton<ExecutionEngine>();
        services.AddHostedService<WorkflowEngineWorker>();
    }
}
