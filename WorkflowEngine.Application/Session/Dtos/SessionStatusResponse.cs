namespace WorkflowEngine.Application.Session.Dtos;


/// <summary>
/// Session status information
/// </summary>
public record SessionStatusResponse
{
    public Guid SessionId { get; init; }
    public string Username { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public bool IsPaused { get; init; }
    public int CallDepth { get; init; }
    public object? Dialog { get; init; }  // Current dialog if paused
}
