using System;
using System.Collections.Generic;
using System.Text;
using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;
using WorkflowEngine.Domain.ProcessEngine.Enums;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Executors
{
    internal class CalculateExecutor : IActionExecutor
    {
        public static readonly CalculateExecutor Instance = new();

        private CalculateExecutor() { }

        public async Task<ActionResult> ExecuteAsync(ExecutionSession session, Guid applicationId, Guid moduleId)
        {
            try
            {
                var calcModule = session.ModuleCache.GetModule(applicationId, moduleId) as CalculateActionModule;
                if (calcModule == null)
                {
                    return ActionResult.Fail($"Calculate action module {moduleId} not found");
                }
                
                foreach (var detail in calcModule.Details.OrderBy(d => d.Sequence))
                {
                    ExecuteCalculation(detail, session);
                }
                return ActionResult.Pass("Calculations completed");
            }
            catch (Exception ex)
            {
                return ActionResult.Fail($"Calculations Execution Failed: {ex.Message}", ex);
            }
        }

        private void ExecuteCalculation(CalculateModuleDetail detail, ExecutionSession session)
        {
            // Get input values
            var input1 = GetValue(detail.Input1IsConstant, detail.Input1FieldId, detail.Input1Value, session);
            var input2 = GetValue(detail.Input2IsConstant, detail.Input2FieldId, detail.Input2Value, session);

            object result;

            switch (detail.OperatorId)
            {
                case CalculateOperator.Assign:
                    result = input1;
                    break;

                case CalculateOperator.Concatenate:
                    result = $"{input1}{input2}";
                    break;

                case CalculateOperator.Add:
                    result = ConvertToDecimal(input1) + ConvertToDecimal(input2);
                    break;

                case CalculateOperator.Subtract:
                    result = ConvertToDecimal(input1) - ConvertToDecimal(input2);
                    break;

                case CalculateOperator.Multiply:
                    result = ConvertToDecimal(input1) * ConvertToDecimal(input2);
                    break;

                case CalculateOperator.Divide:
                    var divisor = ConvertToDecimal(input2);
                    if (divisor == 0)
                        throw new DivideByZeroException("Cannot divide by zero");
                    result = ConvertToDecimal(input1) / divisor;
                    break;

                case CalculateOperator.Modulus:
                    result = ConvertToDecimal(input1) % ConvertToDecimal(input2);
                    break;

                case CalculateOperator.Clear:
                    session.RemoveFieldValue(detail.ResultFieldId);
                    return;

                default:
                    throw new InvalidOperationException($"Unknown operator: {detail.OperatorId}");
            }

            // Store result in target field
            session.SetFieldValue(detail.ResultFieldId, result);
        }

        private object? GetValue(bool isConstant, Guid? fieldId, string constantValue, ExecutionSession session)
        {
            if (isConstant) return constantValue;

            if (fieldId.HasValue) return session.GetFieldValue(fieldId.Value);

            return null;
        }
        private decimal ConvertToDecimal(object? value)
        {
            if (value == null) return 0;
            if (value is decimal d) return d;
            if (decimal.TryParse(value.ToString(), out var result))
                return result;
            return 0;
        }

    }
}
