using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Application.Session.Dtos;
using WorkflowEngine.Application.Session.Services;

namespace WorkflowEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(
        ISessionService sessionService,
        ILogger<WorkflowController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Start a new workflow session or connect to existing session
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<StartWorkflowResponse>> StartWorkflow(
        [FromBody] StartWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _sessionService.StartWorkflowAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in StartWorkflow endpoint");
            return StatusCode(500, new { Error = "Failed to start workflow", Details = ex.Message });
        }
    }

    /// <summary>
    /// Resume a paused workflow by submitting user input
    /// </summary>
    [HttpPost("resume")]
    public async Task<ActionResult<ResumeWorkflowResponse>> ResumeWorkflow(
        [FromBody] ResumeWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _sessionService.ResumeWorkflowAsync(request, cancellationToken);

            if (response.Status == "NotFound")
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ResumeWorkflow endpoint");
            return StatusCode(500, new { Error = "Failed to resume workflow", Details = ex.Message });
        }
    }

    /// <summary>
    /// Get session status and current state
    /// </summary>
    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<SessionStatusResponse>> GetSessionStatus(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _sessionService.GetSessionStatusAsync(sessionId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Error = "Session not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSessionStatus endpoint");
            return StatusCode(500, new { Error = "Failed to get session status", Details = ex.Message });
        }
    }

    /// <summary>
    /// Get user's active session (if any)
    /// </summary>
    [HttpGet("user/{username}/session")]
    public async Task<ActionResult<SessionStatusResponse>> GetUserSession(
        string username,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _sessionService.GetUserSessionAsync(username, cancellationToken);

            if (response == null)
            {
                return NotFound(new { Message = "No active session for user" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserSession endpoint");
            return StatusCode(500, new { Error = "Failed to get user session", Details = ex.Message });
        }
    }

    /// <summary>
    /// Cancel/abandon a session
    /// </summary>
    [HttpDelete("session/{sessionId}")]
    public async Task<ActionResult> CancelSession(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _sessionService.CancelSessionAsync(sessionId, cancellationToken);

            if (!success)
            {
                return NotFound(new { Error = "Session not found" });
            }

            return Ok(new { Message = "Session cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CancelSession endpoint");
            return StatusCode(500, new { Error = "Failed to cancel session", Details = ex.Message });
        }
    }
}
