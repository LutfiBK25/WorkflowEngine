using WorkflowEngine.Domain.ProcessEngine.Enums;


namespace WorkflowEngine.Domain.ProcessEngine.Entities.Modules;

public class CalculateActionModule : Module
{
    public List<CalculateModuleDetail> Details { get; set; } = new();

    public CalculateActionModule()
    {
        ModuleType = ModuleType.CalculateAction;
    }
}

public class CalculateModuleDetail
{
    public Guid Id { get; set; }
    public Guid CalculateActionId { get; set; }
    public int Sequence { get; set; }

    public CalculateOperator OperatorId { get; set; }

    // Input 1
    public bool Input1IsConstant { get; set; }
    public Guid? Input1FieldId { get; set; }
    public string Input1Value { get; set; } = string.Empty;

    // Input 2
    public bool Input2IsConstant { get; set; }
    public Guid? Input2FieldId { get; set; }
    public string Input2Value { get; set; } = string.Empty;

    // Result
    public Guid ResultFieldId { get; set; }
    public CalculateActionModule CalculateActionModule { get; set; }
}
