using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;
using WorkflowEngine.Domain.ProcessEngine.Enums;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Executors;

public class CompareExecutor : IActionExecutor
{
    public static readonly CompareExecutor Instance = new();

    private CompareExecutor() { }

    public async Task<ActionResult> ExecuteAsync(
        ExecutionSession session, 
        Guid applicationId, 
        Guid moduleId)
    {
        try
        {
            var cmpModule = session.ModuleCache.GetModule(applicationId, moduleId) as CompareActionModule;
            if(cmpModule == null)
            {
                return ActionResult.Fail($"Compare action module {moduleId} not found");
            }

            var value1 = GetValue(cmpModule.Input1IsConstant, cmpModule.Input1FieldId, cmpModule.Input1Value, session);
            var value2 = GetValue(cmpModule.Input2IsConstant, cmpModule.Input2FieldId, cmpModule.Input2Value, session);

            if(value1 is null ||  value2 is null)
            {
                return ActionResult.Fail($"One of the Compare values is Null");
            }

            // Return pass if true, fail if false
            bool comparisonResult = PerformComparison(value1, value2, cmpModule.OperatorId);

            return comparisonResult 
                ? ActionResult.Pass("Comparison Passed") 
                : ActionResult.Fail("Comparison Failed");
        }
        catch (Exception ex)
        {
            return ActionResult.Fail($"Compare Execution Failed: {ex.Message}", ex);
        }
    }

    private object? GetValue(bool isConstant, Guid? fieldId, string constantValue, ExecutionSession session)
    {
        if (isConstant) return constantValue;

        if (fieldId.HasValue) return session.GetFieldValue(fieldId.Value);

        return null;
    }

    private bool PerformComparison(object value1, object value2, CompareOperator operatorId)
    {
        var str1 = value1?.ToString() ?? "";
        var str2 = value2?.ToString() ?? "";

        switch (operatorId)
        {
            case CompareOperator.Equals:
                return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);

            case CompareOperator.NotEquals:
                return !str1.Equals(str2, StringComparison.OrdinalIgnoreCase);

            case CompareOperator.GreaterThan:
                return CompareNumeric(value1, value2) > 0;

            case CompareOperator.LessThan:
                return CompareNumeric(value1, value2) < 0;

            case CompareOperator.GreaterThanOrEqual:
                return CompareNumeric(value1, value2) >= 0;

            case CompareOperator.LessThanOrEqual:
                return CompareNumeric(value1, value2) <= 0;

            case CompareOperator.Contains:
                return str1.Contains(str2, StringComparison.OrdinalIgnoreCase);

            case CompareOperator.StartsWith:
                return str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase);

            case CompareOperator.EndsWith:
                return str1.EndsWith(str2, StringComparison.OrdinalIgnoreCase);

            default:
                throw new InvalidOperationException($"Unknown comparison operator: {operatorId}");
        }
    }

    private int CompareNumeric(object value1, object value2)
    {
        if (decimal.TryParse(value1?.ToString(), out var num1) &&
            decimal.TryParse(value2?.ToString(), out var num2))
        {
            return num1.CompareTo(num2);
        }

        // Fall back to string comparison
        return string.Compare(
            value1?.ToString(),
            value2?.ToString(),
            StringComparison.OrdinalIgnoreCase);
    }
}
