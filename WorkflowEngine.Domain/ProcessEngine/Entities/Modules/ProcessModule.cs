

using WorkflowEngine.Domain.ProcessEngine.Enums;

namespace WorkflowEngine.Domain.ProcessEngine.Entities.Modules
{
    public class ProcessModule : Module
    {
        public string? Comment { get; set; }
        public List<ProcessModuleDetail> Details { get; set; } = new();
        public ProcessModule() { 
        ModuleType = ModuleType.ProcessModule;
        }
    }
    public class ProcessModuleDetail
    {
        public Guid Id { get; set; }
        public Guid ProcessModuleId { get; set; }
        public int Sequence { get; set; }
        public string? LabelName { get; set; }
        public ActionType? ActionType { get; set; }
        public Guid? ModuleId { get; set; }
        public ModuleType? ActionModuleType { get; set; }
        public string? PassLabel { get; set; }
        public string? FailLabel { get; set; }
        public bool CommentedFlag { get; set; }
        public string? Comment { get; set; }

        public ProcessModule ProcessModule { get; set; }
    }
}
