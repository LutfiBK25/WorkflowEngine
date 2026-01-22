namespace WorkflowEngine.Application.Session.Dtos;

/// <summary>
/// Response from starting/connecting to session
/// </summary>
public record StartWorkflowResponse
{
    public Guid SessionId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;  // "NewSession", "ExistingSession", "Paused", "Completed"
    public bool IsExistingSession { get; init; }
    public string? Message { get; init; }
    public object? Dialog { get; init; }  // Parsed dialog JSON if paused
    public bool Success { get; init; }
}
