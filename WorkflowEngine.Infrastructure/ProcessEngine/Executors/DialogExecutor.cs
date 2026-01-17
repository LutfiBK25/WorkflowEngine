
using System.Text.Json;
using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Executors;

public class DialogExecutor : IActionExecutor
{
    // Singleton instance - no state, no constructor dependencies
    public static readonly DialogExecutor Instance = new();
    private DialogExecutor() { } // Private constructor


    public async Task<ActionResult> ExecuteAsync(ExecutionSession session,
        Guid applicationId,
        Guid moduleId)
    {
        try
        {
            // Get dialog action module
            var dialogModule = session.ModuleCache.GetModule(applicationId, moduleId) as DialogActionModule;
            if (dialogModule == null)
            {
                return ActionResult.Fail($"Dialog action module {moduleId} not found");
            }

            // Get the field module that will receive the user input
            var fieldModule = session.ModuleCache.GetModule(applicationId, dialogModule.FieldModuleId) as FieldModule;
            if (fieldModule == null)
            {
                return ActionResult.Fail($"Field module {dialogModule.FieldModuleId} not found for dialog '{dialogModule.Name}'");
            }

            // Generate dialog screen JSON with field metadata
            var dialogJson = GenerateDialogJson(dialogModule, fieldModule, session);

            // Pause execution at current step
            if (session.CurrentFrame == null)
            {
                return ActionResult.Fail("No execution frame on stack - cannot pause");
            }


            session.Pause(
                session.CurrentFrame.ProcessId,
                session.CurrentFrame.CurrentSequence,
                dialogJson
            );

            // Return success - execution is now paused
            return ActionResult.Pass(
                $"Dialog '{dialogModule.Name}' shown - waiting for input for field '{fieldModule.Name}'"
            );
        }
        catch (Exception ex)
        {
            return ActionResult.Fail($"Dialog execution failed: {ex.Message}", ex);
        }
    }
    /// <summary>
    /// Generate JSON describing the dialog for UI rendering
    /// </summary>
    private string GenerateDialogJson(
        DialogActionModule dialogModule,
        FieldModule fieldModule,
        ExecutionSession session)
    {
        var currentValue = session.GetFieldValue(fieldModule.Id);

        var dialogInfo = new
        {
            // Dialog metadata
            DialogId = dialogModule.Id,
            DialogName = dialogModule.Name,
            DialogDescription = dialogModule.Description,

            // Field metadata for input
            FieldId = fieldModule.Id,
            FieldName = fieldModule.Name,
            FieldType = fieldModule.FieldType.ToString(),
            FieldDescription = fieldModule.Description,

            // Values
            DefaultValue = fieldModule.DefaultValue,
            CurrentValue = currentValue,

            // UI hints
            Prompt = $"Please enter {fieldModule.Name}:",
            IsRequired = true,

            // Session context
            SessionId = session.SessionId,
            UserId = session.UserId
        };

        return JsonSerializer.Serialize(dialogInfo, new JsonSerializerOptions
        {
            WriteIndented = true  // Pretty-print for debugging
        });
    }
}
