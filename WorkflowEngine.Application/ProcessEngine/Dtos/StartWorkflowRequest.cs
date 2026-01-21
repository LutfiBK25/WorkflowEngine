
namespace WorkflowEngine.Application.ProcessEngine.Dtos;

/// <summary>
/// Request to start a new workflow session
/// </summary>
public record StartWorkflowRequest
{
    public Guid ApplicationId { get; init; }
    public Guid ProcessModuleId { get; init; }
    public string Username { get; init; } = string.Empty;
}
