using WorkflowEngine.Domain.ProcessEngine.Enums;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Executors;

public class ActionExecutorRegistry
{
    private static readonly Dictionary<ActionType, IActionExecutor> _executors = new()
{
    { ActionType.Call, ProcessModuleExecutor.Instance },
    { ActionType.Dialog, DialogExecutor.Instance },
    { ActionType.DatabaseExecute, DatabaseActionExecutor.Instance },
    { ActionType.Compare, CompareExecutor.Instance },
    {ActionType.Calcualte, CalculateExecutor.Instance },
};

    /// <summary>
    /// Get executor for a module type
    /// </summary>
    public static IActionExecutor GetExecutor(ActionType actionType)
    {
        if (_executors.TryGetValue(actionType, out var executor))
        {
            return executor;
        }

        throw new NotSupportedException($"No executor registered for module type: {actionType}");
    }

    /// <summary>
    /// Register a custom executor (for extensibility)
    /// </summary>
    public static void RegisterExecutor(ActionType actionType, IActionExecutor executor)
    {
        _executors[actionType] = executor;
    }
}
