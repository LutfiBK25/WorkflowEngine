using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine.Application.Session.Services;

namespace WorkflowEngine.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISessionService, SessionService>();
    }
}