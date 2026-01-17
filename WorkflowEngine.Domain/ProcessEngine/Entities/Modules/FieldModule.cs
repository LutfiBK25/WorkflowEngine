using WorkflowEngine.Domain.ProcessEngine.Enums;

namespace WorkflowEngine.Domain.ProcessEngine.Entities.Modules;

public class FieldModule : Module
{
    public FieldType FieldType { get; set; }
    public string? DefaultValue { get; set; }

    public FieldModule()
    {
        ModuleType = ModuleType.FieldModule;
    }
}
