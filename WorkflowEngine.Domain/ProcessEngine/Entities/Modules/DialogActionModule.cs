using WorkflowEngine.Domain.ProcessEngine.Enums;


namespace WorkflowEngine.Domain.ProcessEngine.Entities.Modules
{
    public class DialogActionModule : Module
    {
        public Guid FieldModuleId { get; set; } // The field that will be assigned a value

        public DialogActionModule() {
            ModuleType = ModuleType.DialogAction;
        }
    }
}
