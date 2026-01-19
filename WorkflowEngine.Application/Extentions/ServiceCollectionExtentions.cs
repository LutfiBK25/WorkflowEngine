
using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine.Application.ProcessEngine.Services;
using WorkflowEngine.Application.Session.Services;

namespace WorkflowEngine.Application.Extentions;

public static class ServiceCollectionExtentions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProcessEngineService, ProcessEngineService>();
        services.AddScoped<ISessionService, SessionService>();
    }
}
