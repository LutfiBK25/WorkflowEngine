using Microsoft.Extensions.Logging;
using WorkflowEngine.Application.Session.Interfaces;
using WorkflowEngine.Infrastructure.ProcessEngine;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;

namespace WorkflowEngine.Infrastructure.Session;

public class SessionManager : ISessionManager  // ✅ Implements interface
{
    private readonly ISessionStore _sessionStore;
    private readonly ExecutionEngine _executionEngine;
    private readonly ILogger<SessionManager> _logger;

    public SessionManager(
        ISessionStore sessionStore,
        ExecutionEngine executionEngine,
        ILogger<SessionManager> logger)
    {
        _sessionStore = sessionStore;
        _executionEngine = executionEngine;
        _logger = logger;
    }

    public async Task<StartWorkflowResult> StartWorkflowAsync(
        Guid applicationId,
        Guid processModuleId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var existingSession = await _sessionStore.GetUserSessionAsync(userId, cancellationToken);

        if (existingSession != null)
        {
            var status = DetermineStartStatus(existingSession, isExisting: true);
            
            return new StartWorkflowResult(
                existingSession.SessionId,
                existingSession.UserId,
                existingSession.IsPaused,
                existingSession.PausedScreenJson,
                status,
                IsExisting: true,
                Message: $"Connected to existing session. Status: {(existingSession.IsPaused ? "Paused" : "Active")}",
                Success: true
            );
        }

        var session = new ExecutionSession(
            applicationId,
            processModuleId,
            userId,
            _executionEngine.Cache,
            _executionEngine.ConnectionStrings
        );

        var result = await session.Start();

        if (session.IsPaused)
        {
            await _sessionStore.SaveSessionAsync(session, cancellationToken);
        }

        var newStatus = DetermineStartStatus(session, isExisting: false);

        return new StartWorkflowResult(
            session.SessionId,
            session.UserId,
            session.IsPaused,
            session.PausedScreenJson,
            newStatus,
            IsExisting: false,
            Message: result.Message ?? "Workflow started",
            Success: result.Result == ExecutionResult.Success
        );
    }

    public async Task<ResumeWorkflowResult> ResumeWorkflowAsync(
        Guid sessionId,
        Guid fieldId,
        object value,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionStore.GetSessionAsync(sessionId, cancellationToken);

        if (session == null)
        {
            return new ResumeWorkflowResult(
                sessionId,
                IsPaused: false,
                PausedScreenJson: null,
                Message: "Session not found",
                Success: false
            );
        }

        if (!session.CanResume())
        {
            return new ResumeWorkflowResult(
                session.SessionId,
                session.IsPaused,
                session.PausedScreenJson,
                Message: "Session is not in a resumable state",
                Success: false
            );
        }

        session.SetFieldValue(fieldId, value);
        var result = await session.Start();

        if (session.IsPaused)
        {
            await _sessionStore.SaveSessionAsync(session, cancellationToken);
        }
        else
        {
            await _sessionStore.RemoveSessionAsync(sessionId, cancellationToken);
        }

        return new ResumeWorkflowResult(
            session.SessionId,
            session.IsPaused,
            session.PausedScreenJson,
            Message: result.Message ?? "Workflow resumed",
            Success: result.Result == ExecutionResult.Success
        );
    }

    public async Task<SessionInfo?> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionStore.GetSessionAsync(sessionId, cancellationToken);

        if (session == null)
            return null;

        return new SessionInfo(
            session.SessionId,
            session.UserId,
            session.StartTime,
            session.IsPaused,
            session.CallDepth,
            session.PausedScreenJson
        );
    }

    public async Task<SessionInfo?> GetUserSessionAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionStore.GetUserSessionAsync(userId, cancellationToken);

        if (session == null)
            return null;

        return new SessionInfo(
            session.SessionId,
            session.UserId,
            session.StartTime,
            session.IsPaused,
            session.CallDepth,
            session.PausedScreenJson
        );
    }

    public async Task<bool> CancelSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionStore.GetSessionAsync(sessionId, cancellationToken);
        if (session == null)
            return false;

        await _sessionStore.RemoveSessionAsync(sessionId, cancellationToken);
        return true;
    }

    public async Task<bool> CancelUserSessionAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionStore.GetUserSessionAsync(userId, cancellationToken);
        if (session == null)
            return false;

        await _sessionStore.RemoveUserSessionAsync(userId, cancellationToken);
        return true;
    }

    public Task CleanupExpiredSessionsAsync(
        TimeSpan sessionInActiveMaxAge,
        CancellationToken cancellationToken = default)
    {
        return _sessionStore.CleanupExpiredSessionsAsync(sessionInActiveMaxAge, cancellationToken);
    }

    private string DetermineStartStatus(ExecutionSession session, bool isExisting)
    {
        if (isExisting && session.IsPaused)
            return "ExistingSessionPaused";
        if (isExisting)
            return "ExistingSessionActive";
        if (session.IsPaused)
            return "NewSessionPaused";
        return "Completed";
    }
}
