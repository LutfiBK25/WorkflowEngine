
using WorkflowEngine.Infrastructure.ProcessEngine;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;

namespace WorkflowEngine.Infrastructure.Session;

/// <summary>
/// Manages workflow execution sessions
/// ONE SESSION PER USER constraint enforced
/// </summary>
public class SessionManager
{
    private readonly ISessionStore _sessionStore;
    private readonly ExecutionEngine _executionEngine;

    public SessionManager(ISessionStore sessionStore, ExecutionEngine executionEngine)
    {
        _sessionStore = sessionStore;
        _executionEngine = executionEngine;
    }

    /// <summary>
    /// Create and start a new workflow session
    /// </summary>
    public async Task<(ExecutionSession Session, ActionResult Result, bool IsExisting)> StartWorkflowAsync(
        Guid applicationId,
        Guid processModuleId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Check if user already has an active session
        var existingSession = await _sessionStore.GetUserSessionAsync(userId, cancellationToken);

        if (existingSession != null)
        {
            // User already has a session - return it instead of creating new one
            return (
                existingSession,
                ActionResult.Pass($"Connected to existing session. Status: {(existingSession.IsPaused ? "Paused" : "Active")}"),
                IsExisting: true
            );
        }

        // No existing session - create new one
        var session = new ExecutionSession(
            applicationId,
            processModuleId,
            userId,
            _executionEngine.Cache,
            _executionEngine.ConnectionStrings
        );

        var result = await session.Start();

        // Save session if paused (waiting for input) or still running
        if (session.IsPaused)
        {
            await _sessionStore.SaveSessionAsync(session, cancellationToken);
        }

        return (session, result, IsExisting: false);
    }

    /// <summary>
    /// Resume a paused session with user input
    /// </summary>
    public async Task<(ExecutionSession? Session, ActionResult Result)> ResumeWorkflowAsync(
        Guid sessionId,
        Guid fieldId,
        object value,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionStore.GetSessionAsync(sessionId, cancellationToken);

        if (session == null)
        {
            return (null, ActionResult.Fail("Session not found"));
        }

        if (!session.CanResume())
        {
            return (session, ActionResult.Fail("Session is not in a resumable state"));
        }

        // Store user input
        session.SetFieldValue(fieldId, value);

        // Resume execution
        var result = await session.Start();

        // Update or remove session
        if (session.IsPaused)
        {
            // Still paused (another dialog)
            await _sessionStore.SaveSessionAsync(session, cancellationToken);
        }
        else
        {
            // Completed - remove from storage
            await _sessionStore.RemoveSessionAsync(sessionId, cancellationToken);
        }

        return (session, result);
    }

    /// <summary>
    /// Get session by ID
    /// </summary>
    public Task<ExecutionSession?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return _sessionStore.GetSessionAsync(sessionId, cancellationToken);
    }

    /// <summary>
    /// Get user's active session (returns null if no active session)
    /// </summary>
    public Task<ExecutionSession?> GetUserSessionAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _sessionStore.GetUserSessionAsync(userId, cancellationToken);
    }



    /// <summary>
    /// Cancel/abandon a session by session ID
    /// </summary>
    public async Task<bool> CancelSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionStore.GetSessionAsync(sessionId, cancellationToken);
        if (session == null)
            return false;

        await _sessionStore.RemoveSessionAsync(sessionId, cancellationToken);
        return true;
    }

    /// <summary>
    /// Cancel/abandon a user's active session
    /// </summary>
    public async Task<bool> CancelUserSessionAsync(string userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionStore.GetUserSessionAsync(userId, cancellationToken);
        if (session == null)
            return false;

        await _sessionStore.RemoveUserSessionAsync(userId, cancellationToken);
        return true;
    }

    /// <summary>
    /// Cleanup expired sessions
    /// </summary>
    public Task CleanupExpiredSessionsAsync(TimeSpan sessionInActiveMaxAge, CancellationToken cancellationToken = default)
    {
        return _sessionStore.CleanupExpiredSessionsAsync(sessionInActiveMaxAge, cancellationToken);
    }

}
