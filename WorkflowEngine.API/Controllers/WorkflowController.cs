using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WorkflowEngine.Application.ProcessEngine.Dtos;
using WorkflowEngine.Application.Session.Dtos;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;
using WorkflowEngine.Infrastructure.Session;

namespace WorkflowEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly SessionManager _sessionManager;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(SessionManager sessionManager, ILogger<WorkflowController> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    /// <summary>
    /// Start a new workflow session or connect to existing session
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<StartWorkflowResponse>> StartWorkflow(
        [FromBody] StartWorkflowRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Starting workflow for user: {Username}, App: {AppId}, Process: {ProcessId}",
                request.Username, request.ApplicationId, request.ProcessModuleId);

            var (session, result, isExisting) = await _sessionManager.StartWorkflowAsync(
                request.ApplicationId,
                request.ProcessModuleId,
                request.Username
            );

            // Parse dialog JSON if paused
            object? dialog = null;
            if (session.IsPaused && !string.IsNullOrEmpty(session.PausedScreenJson))
            {
                try
                {
                    dialog = JsonSerializer.Deserialize<object>(session.PausedScreenJson);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse dialog JSON");
                }
            }

            string status = DetermineStatus(session, isExisting);

            var response = new StartWorkflowResponse
            {
                SessionId = session.SessionId,
                Username = session.UserId,
                Status = status,
                IsExistingSession = isExisting,
                Message = result.Message,
                Dialog = dialog,
                Success = result.Result == ExecutionResult.Success
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting workflow for user: {Username}", request.Username);
            return StatusCode(500, new { Error = "Failed to start workflow", Details = ex.Message });
        }
    }

    /// <summary>
    /// Resume a paused workflow by submitting user input
    /// </summary>
    [HttpPost("resume")]
    public async Task<ActionResult<ResumeWorkflowResponse>> ResumeWorkflow(
        [FromBody] ResumeWorkflowRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Resuming session: {SessionId}, Field: {FieldId}, Value: {Value}",
                request.SessionId, request.FieldId, request.Value);

            var (session, result) = await _sessionManager.ResumeWorkflowAsync(
                request.SessionId,
                request.FieldId,
                request.Value
            );

            if (session == null)
            {
                return NotFound(new ResumeWorkflowResponse
                {
                    SessionId = request.SessionId,
                    Status = "NotFound",
                    Message = "Session not found or expired",
                    Success = false
                });
            }

            // Parse dialog JSON if paused again
            object? dialog = null;
            if (session.IsPaused && !string.IsNullOrEmpty(session.PausedScreenJson))
            {
                try
                {
                    dialog = JsonSerializer.Deserialize<object>(session.PausedScreenJson);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse dialog JSON");
                }
            }

            string status = session.IsPaused ? "Paused" : "Completed";

            var response = new ResumeWorkflowResponse
            {
                SessionId = session.SessionId,
                Status = status,
                Message = result.Message,
                Dialog = dialog,
                Success = result.Result == ExecutionResult.Success
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming session: {SessionId}", request.SessionId);
            return StatusCode(500, new { Error = "Failed to resume workflow", Details = ex.Message });
        }
    }

    /// <summary>
    /// Get session status and current state
    /// </summary>
    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<SessionStatusResponse>> GetSessionStatus(Guid sessionId)
    {
        try
        {
            var session = await _sessionManager.GetSessionAsync(sessionId);

            if (session == null)
            {
                return NotFound(new { Error = "Session not found" });
            }

            // Parse dialog JSON if paused
            object? dialog = null;
            if (session.IsPaused && !string.IsNullOrEmpty(session.PausedScreenJson))
            {
                try
                {
                    dialog = JsonSerializer.Deserialize<object>(session.PausedScreenJson);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse dialog JSON");
                }
            }

            var response = new SessionStatusResponse
            {
                SessionId = session.SessionId,
                Username = session.UserId,
                StartTime = session.StartTime,
                IsPaused = session.IsPaused,
                CallDepth = session.CallDepth,
                Dialog = dialog
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session status: {SessionId}", sessionId);
            return StatusCode(500, new { Error = "Failed to get session status", Details = ex.Message });
        }
    }

    /// <summary>
    /// Cancel/abandon a session
    /// </summary>
    [HttpDelete("session/{sessionId}")]
    public async Task<Microsoft.AspNetCore.Mvc.ActionResult> CancelSession(Guid sessionId)
    {
        try
        {
            var success = await _sessionManager.CancelSessionAsync(sessionId);

            if (!success)
            {
                return NotFound(new { Error = "Session not found" });
            }

            return Ok(new { Message = "Session cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling session: {SessionId}", sessionId);
            return StatusCode(500, new { Error = "Failed to cancel session", Details = ex.Message });
        }
    }

    /// <summary>
    /// Get user's active session (if any)
    /// </summary>
    [HttpGet("user/{username}/session")]
    public async Task<ActionResult<SessionStatusResponse>> GetUserSession(string username)
    {
        try
        {
            var session = await _sessionManager.GetUserSessionAsync(username);

            if (session == null)
            {
                return NotFound(new { Message = "No active session for user" });
            }

            // Parse dialog JSON if paused
            object? dialog = null;
            if (session.IsPaused && !string.IsNullOrEmpty(session.PausedScreenJson))
            {
                try
                {
                    dialog = JsonSerializer.Deserialize<object>(session.PausedScreenJson);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse dialog JSON");
                }
            }

            var response = new SessionStatusResponse
            {
                SessionId = session.SessionId,
                Username = session.UserId,
                StartTime = session.StartTime,
                IsPaused = session.IsPaused,
                CallDepth = session.CallDepth,
                Dialog = dialog
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user session: {Username}", username);
            return StatusCode(500, new { Error = "Failed to get user session", Details = ex.Message });
        }
    }

    private string DetermineStatus(
        Infrastructure.ProcessEngine.Execution.ExecutionSession session,
        bool isExisting)
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
