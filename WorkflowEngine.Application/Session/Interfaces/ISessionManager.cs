namespace WorkflowEngine.Application.Session.Interfaces;

/// <summary>
/// Session management interface - defined in Application, implemented in Infrastructure
/// This enables Dependency Inversion Principle
/// </summary>
public interface ISessionManager
{
    Task<StartWorkflowResult> StartWorkflowAsync(
        Guid applicationId,
        Guid processModuleId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<ResumeWorkflowResult> ResumeWorkflowAsync(
        Guid sessionId,
        Guid fieldId,
        object value,
        CancellationToken cancellationToken = default);

    Task<SessionInfo?> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<SessionInfo?> GetUserSessionAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> CancelSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<bool> CancelUserSessionAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task CleanupExpiredSessionsAsync(
        TimeSpan maxAge,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from starting a workflow - Application layer type
/// </summary>
public record StartWorkflowResult(
    Guid SessionId,
    string UserId,
    bool IsPaused,
    string? PausedScreenJson,
    string Status, // Calculated status string
    bool IsExisting,
    string Message,
    bool Success
);

/// <summary>
/// Result from resuming a workflow
/// </summary>
public record ResumeWorkflowResult(
    Guid SessionId,
    bool IsPaused,
    string? PausedScreenJson,
    string Message,
    bool Success
);

/// <summary>
/// Session information - Application layer type
/// </summary>
public record SessionInfo(
    Guid SessionId,
    string UserId,
    DateTime StartTime,
    bool IsPaused,
    int CallDepth,
    string? PausedScreenJson
);