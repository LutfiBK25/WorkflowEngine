using System.Text.Json;
using Microsoft.Extensions.Logging;
using WorkflowEngine.Application.Session.Dtos;
using WorkflowEngine.Application.Session.Interfaces;

namespace WorkflowEngine.Application.Session.Services;

public class SessionService : ISessionService
{
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<SessionService> _logger;

    public SessionService(
        ISessionManager sessionManager,
        ILogger<SessionService> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    public async Task<StartWorkflowResponse> StartWorkflowAsync(
        StartWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Starting workflow for user: {Username}, App: {AppId}, Process: {ProcessId}",
                request.Username, request.ApplicationId, request.ProcessModuleId);

            var result = await _sessionManager.StartWorkflowAsync(
                request.ApplicationId,
                request.ProcessModuleId,
                request.Username,
                cancellationToken
            );

            // Parse dialog JSON if paused
            object? dialog = null;
            if (result.IsPaused && !string.IsNullOrEmpty(result.PausedScreenJson))
            {
                try
                {
                    dialog = JsonSerializer.Deserialize<object>(result.PausedScreenJson);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse dialog JSON");
                }
            }

            return new StartWorkflowResponse
            {
                SessionId = result.SessionId,
                Username = result.UserId,
                Status = result.Status,
                IsExistingSession = result.IsExisting,
                Message = result.Message,
                Dialog = dialog,
                Success = result.Success
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting workflow for user: {Username}", request.Username);
            throw;
        }
    }

    public async Task<ResumeWorkflowResponse> ResumeWorkflowAsync(
        ResumeWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Resuming session: {SessionId}, Field: {FieldId}",
                request.SessionId, request.FieldId);

            var result = await _sessionManager.ResumeWorkflowAsync(
                request.SessionId,
                request.FieldId,
                request.Value,
                cancellationToken
            );

            if (!result.Success && result.Message == "Session not found")
            {
                return new ResumeWorkflowResponse
                {
                    SessionId = request.SessionId,
                    Status = "NotFound",
                    Message = result.Message,
                    Dialog = null,
                    Success = false
                };
            }

            object? dialog = null;
            if (result.IsPaused && !string.IsNullOrEmpty(result.PausedScreenJson))
            {
                try
                {
                    dialog = JsonSerializer.Deserialize<object>(result.PausedScreenJson);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse dialog JSON");
                }
            }

            string status = result.IsPaused ? "Paused" : "Completed";

            return new ResumeWorkflowResponse
            {
                SessionId = result.SessionId,
                Status = status,
                Message = result.Message,
                Dialog = dialog,
                Success = result.Success
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming session: {SessionId}", request.SessionId);
            throw;
        }
    }

    public async Task<SessionStatusResponse> GetSessionStatusAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionManager.GetSessionAsync(sessionId, cancellationToken);

        if (session == null)
        {
            throw new KeyNotFoundException($"Session {sessionId} not found");
        }

        object? dialog = null;
        if (session.IsPaused && !string.IsNullOrEmpty(session.PausedScreenJson))
        {
            try
            {
                dialog = JsonSerializer.Deserialize<object>(session.PausedScreenJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse dialog JSON for session {SessionId}", sessionId);
            }
        }

        return new SessionStatusResponse
        {
            SessionId = session.SessionId,
            Username = session.UserId,
            StartTime = session.StartTime,
            IsPaused = session.IsPaused,
            CallDepth = session.CallDepth,
            Dialog = dialog
        };
    }

    public async Task<SessionStatusResponse?> GetUserSessionAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionManager.GetUserSessionAsync(username, cancellationToken);

        if (session == null)
        {
            return null;
        }

        object? dialog = null;
        if (session.IsPaused && !string.IsNullOrEmpty(session.PausedScreenJson))
        {
            try
            {
                dialog = JsonSerializer.Deserialize<object>(session.PausedScreenJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse dialog JSON for user {Username}", username);
            }
        }

        return new SessionStatusResponse
        {
            SessionId = session.SessionId,
            Username = session.UserId,
            StartTime = session.StartTime,
            IsPaused = session.IsPaused,
            CallDepth = session.CallDepth,
            Dialog = dialog
        };
    }

    public async Task<bool> CancelSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _sessionManager.CancelSessionAsync(sessionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling session: {SessionId}", sessionId);
            throw;
        }
    }
}
