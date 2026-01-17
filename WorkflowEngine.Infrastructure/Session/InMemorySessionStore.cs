

using System.Collections.Concurrent;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;

namespace WorkflowEngine.Infrastructure.Session;

/// <summary>
/// In-memory session store - for development/testing
/// Sessions lost on restart!
/// ONE SESSION PER USER constraint enforced
/// </summary>
public class InMemorySessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<Guid, ExecutionSession> _sessionsBySessionId = new();
    private readonly ConcurrentDictionary<string, Guid> _sessionIdByUserId = new(); 

    public int Count => _sessionsBySessionId.Count;

    /// <summary>
    /// Save or update a session
    /// </summary>
    public Task SaveSessionAsync(ExecutionSession session, CancellationToken cancellationToken = default)
    {
        // Save session
        _sessionsBySessionId[session.SessionId] = session;

        // Update user mapping (one session per user)
        _sessionIdByUserId[session.UserId] = session.SessionId;

        return Task.CompletedTask;
    }

    public Task<ExecutionSession?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _sessionsBySessionId.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    public Task<ExecutionSession?> GetUserSessionAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (_sessionIdByUserId.TryGetValue(userId, out var sessionId))
        {
            _sessionsBySessionId.TryGetValue(sessionId, out var session);
            return Task.FromResult(session);
        }
        return Task.FromResult<ExecutionSession?>(null);
    }


    public Task RemoveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (_sessionsBySessionId.TryRemove(sessionId, out var session))
        {
            // Also remove from user mapping
            _sessionIdByUserId.TryRemove(session.UserId, out _);
        }
        return Task.CompletedTask;
    }

    public Task RemoveUserSessionAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (_sessionIdByUserId.TryRemove(userId, out var sessionId))
        {
            _sessionsBySessionId.TryRemove(sessionId, out _);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Remove all sessions that have Inactive exceeded the specified maximum age
    /// </summary>
    public Task CleanupExpiredSessionsAsync(TimeSpan sessionInActiveMaxAge, CancellationToken cancellationToken = default)
    {
        var expiredTime = DateTime.UtcNow - sessionInActiveMaxAge;
        var expiredSessions = _sessionsBySessionId.Values
            .Where(s => s.LastActive < expiredTime)
            .Select(s => s.SessionId)
            .ToList();

        foreach (var sessionId in expiredSessions)
        {
            if (_sessionsBySessionId.TryRemove(sessionId, out var session))
            {
                _sessionIdByUserId.TryRemove(session.UserId, out _);
            }
        }

        return Task.CompletedTask;
    }
}
