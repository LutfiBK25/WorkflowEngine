

using WorkflowEngine.Domain.ProcessEngine.Enums;

namespace WorkflowEngine.Domain.ProcessEngine.Entities.Modules;

public class CompareActionModule : Module
{
    public CompareOperator OperatorId { get; set; }

    // Input 1
    public bool Input1IsConstant { get; set; }
    public Guid? Input1FieldId { get; set; }
    public string Input1Value { get; set; } = string.Empty;

    // Input 2
    public bool Input2IsConstant { get; set; }
    public Guid? Input2FieldId { get; set; }
    public string Input2Value { get; set; } = string.Empty ;
}
