
namespace WorkflowEngine.Application.ProcessEngine.Dtos;

/// <summary>
/// Request to resume workflow with user input
/// </summary>
public record ResumeWorkflowRequest
{
    public Guid SessionId { get; init; }
    public Guid FieldId { get; init; }
    public string Value { get; init; } = string.Empty;
}
