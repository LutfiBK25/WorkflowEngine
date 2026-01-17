
namespace WorkflowEngine.Domain.ProcessEngine.Entities.Modules;

public class DatabaseActionModule : Module
{
    public string SqlStatement { get; set; }
    
    public DatabaseActionModule()
    {
        ModuleType = Enums.ModuleType.DatabaseAction;
    }

}
