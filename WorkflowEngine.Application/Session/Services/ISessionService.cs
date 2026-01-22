using System;
using System.Threading;
using WorkflowEngine.Application.Session.Dtos;

namespace WorkflowEngine.Application.Session.Services;

/// <summary>
/// Application service for workflow session management
/// </summary>
public interface ISessionService
{
    Task<StartWorkflowResponse> StartWorkflowAsync(
        StartWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<ResumeWorkflowResponse> ResumeWorkflowAsync(
        ResumeWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<SessionStatusResponse> GetSessionStatusAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<SessionStatusResponse?> GetUserSessionAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<bool> CancelSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
