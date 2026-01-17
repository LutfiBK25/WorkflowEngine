
namespace WorkflowEngine.Infrastructure.ProcessEngine.Execution;

public class ExecutionFrame
{
    public Guid ProcessId { get; set; }
    public string ProcessName { get; set; }
    public int CurrentSequence { get; set; }
    public DateTime EnteredAt { get; set; }
}
