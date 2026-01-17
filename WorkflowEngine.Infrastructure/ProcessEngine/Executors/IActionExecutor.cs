using WorkflowEngine.Infrastructure.ProcessEngine.Execution;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Executors
{
    public interface IActionExecutor
    {
        /// <summary>
        /// Execute the action with all context provided as parameters
        /// </summary>
        Task<ActionResult> ExecuteAsync(
            ExecutionSession session,
            Guid applicationId,
            Guid moduleId);
    }
}
