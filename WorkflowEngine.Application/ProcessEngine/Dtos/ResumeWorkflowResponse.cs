

namespace WorkflowEngine.Application.ProcessEngine.Dtos;

public record ResumeWorkflowResponse
{
    public Guid SessionId { get; init; }
    public string Status { get; init; } = string.Empty;  // "Completed", "Paused", "Error"
    public string? Message { get; init; }
    public object? Dialog { get; init; }  // Next dialog if paused again
    public bool Success { get; init; }
}
