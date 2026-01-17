
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;

namespace WorkflowEngine.Infrastructure.Session;

/// <summary>
/// Interface for storing and retriving execution sessions
/// /// ONE SESSION PER USER - Users cannot have multiple concurrent sessions
/// </summary>
public interface ISessionStore
{
    /// <summary>
    /// Save or update a session
    /// </summary>
    Task SaveSessionAsync(ExecutionSession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a session by session ID
    /// </summary>
    Task<ExecutionSession?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the active session for a user (null if no active session)
    /// </summary>
    Task<ExecutionSession?> GetUserSessionAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a session by session ID
    /// </summary>
    Task RemoveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a user's session by user ID
    /// </summary>
    Task RemoveUserSessionAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove expired/abandoned sessions
    /// </summary>
    Task CleanupExpiredSessionsAsync(TimeSpan sessionInActiveMaxAge, CancellationToken cancellationToken = default);
}