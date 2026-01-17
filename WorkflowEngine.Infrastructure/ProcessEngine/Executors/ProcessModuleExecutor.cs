using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;
using WorkflowEngine.Domain.ProcessEngine.Enums;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Executors;

public class ProcessModuleExecutor : IActionExecutor
{
    public static readonly ProcessModuleExecutor Instance = new();
    // To protect against inifinte nesting in process module
    private const int MaxCallDepth = 20;

    private ProcessModuleExecutor() { }

    // Push a new Frame (Process Module) and start executing it
    public async Task<ActionResult> ExecuteAsync(
        ExecutionSession session,
        Guid applicationId,
        Guid moduleId)
    {
        // Check call depth
        if (session.CallDepth >= MaxCallDepth)
        {
            return ActionResult.Fail($"Max call depth ({MaxCallDepth}) exceeded");
        }

        ProcessModule? processModule = null;
        bool framePushed = false;
        int startingSequence = 1;


        try
        {
            // Get process module from cache
            processModule = session.ModuleCache.GetModule(applicationId, moduleId) as ProcessModule;

            if (processModule == null)
            {
                return ActionResult.Fail($"Process module {moduleId} not found");
            }

            // Check if resuming from pause
            if (session.IsPaused &&
                session.PausedAtProcessModuleId == moduleId &&
                session.PausedAtStep.HasValue)
            {
                startingSequence = session.PausedAtStep.Value + 1; // Resume from NEXT step
                session.Resume(); // Clear pause state
                // Don't push frame - it's already on the stack
            }
            else
            {
                // Push new frame for fresh execution
                var frame = new ExecutionFrame
                {
                    ProcessId = processModule.Id,
                    ProcessName = processModule.Name,
                    CurrentSequence = startingSequence,
                    EnteredAt = DateTime.UtcNow
                };
                session.PushFrame(frame);
                framePushed = true;
            }

            // Start executing the Process Module
            var result = await ExecuteStepsFromSequenceAsync(
                processModule,
                session,
                startingSequence);

            // ExecuteStepsFromSequenceAsync pops frame on normal completion
            framePushed = false;

            return result;
        }
        catch (Exception ex)
        {
            // Pop frame on exception (handles both fresh execution and resume)
            if (session.CallDepth > 0 && session.CurrentFrame?.ProcessId == processModule?.Id)
            {
                session.PopFrame();
            }

            return ActionResult.Fail(
                $"Process Module '{processModule?.Name ?? "Unknown"}' execution failed: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Execute steps starting from a given sequence number
    /// Used by both ExecuteAsync (sequence 1) and ResumeAfterDialogAsync (next sequence)
    /// </summary>
    private async Task<ActionResult> ExecuteStepsFromSequenceAsync(
        ProcessModule processModule,
        ExecutionSession session,
        int startingSequence = 1)
    {
        var steps = processModule.Details.OrderBy(d => d.Sequence).ToList();
        var currentSequence = startingSequence;
        var maxIterations = 10000;
        var iterations = 0;

        while (iterations++ < maxIterations)
        {
            var step = steps.FirstOrDefault(s => s.Sequence == currentSequence);
            if (step == null)
            {
                session.PopFrame();
                return ActionResult.Fail($"Step with sequence {currentSequence} not found in process '{processModule.Name}'");
            }

            // Update frame's current sequence
            if (session.CurrentFrame != null)
            {
                session.CurrentFrame.CurrentSequence = currentSequence;
            }


            // Skip commented steps
            if (step.CommentedFlag)
            {
                currentSequence++;
                continue;
            }

            // Handle ReturnPass/ReturnFail directly
            if (step.ActionType == ActionType.ReturnPass)
            {
                session.PopFrame();  // Remove this process from call stack
                return ActionResult.Pass($"Process '{processModule.Name}' completed successfully");
            }

            if (step.ActionType == ActionType.ReturnFail)
            {
                session.PopFrame();  // Remove this process from call stack
                return ActionResult.Fail($"Process '{processModule.Name}' failed");
            }

            // Execute the step and get result
            var result = await ExecuteStepAsync(step, session, currentSequence);

            // If dialog paused execution, return immediately
            if (session.IsPaused)
            {
                return result;
            }

            // Determine next step based on result
            var nextLabel = result.Result == ExecutionResult.Success
                ? step.PassLabel
                : step.FailLabel;

            currentSequence = ResolveNextSequence(steps, currentSequence, nextLabel);

            // End execution by returning -1 as the step
            if (currentSequence == -1)
            {
                session.PopFrame();
                return result; // End execution
            }
        }

        session.PopFrame();
        return ActionResult.Fail($"Maximum iteration limit ({maxIterations}) reached");
    }

    private async Task<ActionResult> ExecuteStepAsync(
        ProcessModuleDetail step,
        ExecutionSession session,
        int currentSequence
        )
    {
        if (!step.ActionType.HasValue)
        {
            return ActionResult.Fail($"Step at sequence {currentSequence} has no action type");
        }

        var actionType = step.ActionType.Value;

        // If module Id is null for actions that require it
        if (!step.ModuleId.HasValue)
        {
            return ActionResult.Fail($"Step at sequence {currentSequence} with action {actionType} has no module ID");
        }

        // Actual step executing
        try
        {
            var executor = ActionExecutorRegistry.GetExecutor(actionType);
            return await executor.ExecuteAsync(session, session.ApplicationId, step.ModuleId.Value);
        }
        catch (NotSupportedException ex)
        {
            return ActionResult.Fail($"No executor found for action type {actionType}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Determine where to go for the next step
    /// </summary>
    private int ResolveNextSequence(
        List<ProcessModuleDetail> steps,
        int currentSequence,
        string? label)
    {
        if (string.IsNullOrEmpty(label))
            return currentSequence + 1;

        return label.ToUpper() switch
        {
            "NEXT" => currentSequence + 1,
            "PREV" => currentSequence - 1,
            _ => steps.FirstOrDefault(s =>
                    s.LabelName?.Equals(label, StringComparison.OrdinalIgnoreCase) == true)
                ?.Sequence ?? -1
        };
    }
}
