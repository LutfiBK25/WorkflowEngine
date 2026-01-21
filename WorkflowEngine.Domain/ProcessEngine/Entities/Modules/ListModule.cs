using WorkflowEngine.Domain.ProcessEngine.Enums;


namespace WorkflowEngine.Domain.ProcessEngine.Entities.Modules;

/// <summary>
/// Stores list/collection data as JSON
/// Used for dialog selection lists, search results, etc.
/// No dedicated executor - populated by DatabaseActions
/// </summary>
public class ListModule : Module
{
    public int? MaxRows { get; set; }
    public ListModule()
    {
        ModuleType = ModuleType.ListModule;
    }
}
