
namespace WorkflowEngine.Infrastructure.ProcessEngine.Execution;

public enum ExecutionResult
{
    Success,
    Fail
}

public class ActionResult
{
    public ExecutionResult Result { get; set; }
    public String Message { get; set; } = string.Empty;
    public Exception Exception { get; set; }
    public Dictionary<string, string> Parameters { get; set; }

    public static ActionResult Pass(string message = null)
    {
        return new ActionResult
        {
            Result = ExecutionResult.Success,
            Message = message,
        };
    }

    public static ActionResult Fail(string message = null, Exception ex = null)
    {
        return new ActionResult
        {
            Result = ExecutionResult.Fail,
            Message = message,
            Exception = ex
        };
    }
}



