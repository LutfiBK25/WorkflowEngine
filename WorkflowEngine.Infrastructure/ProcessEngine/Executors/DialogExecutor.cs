
using System.Text.Json;
using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;
using WorkflowEngine.Domain.ProcessEngine.Enums;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;
using WorkflowEngine.Infrastructure.ProcessEngine.Parsers;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Executors;

public class DialogExecutor : IActionExecutor
{
    // Singleton instance - no state, no constructor dependencies
    public static readonly DialogExecutor Instance = new();
    private DialogExecutor() { } // Private constructor


    public async Task<ActionResult> ExecuteAsync(
        ExecutionSession session,
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

            // Check current frame
            if (session.CurrentFrame == null)
            {
                return ActionResult.Fail("No execution frame on stack - cannot pause");
            }

            // Generate dialog JSON
            var dialogJson = GenerateDialogJson(dialogModule, session);

            session.Pause(
                session.CurrentFrame.ProcessId,
                session.CurrentFrame.CurrentSequence,
                dialogJson
            );

            // Return success - execution is now paused
            return ActionResult.Pass($"Dialog '{dialogModule.Name}' shown");

        }
        catch (Exception ex)
        {
            return ActionResult.Fail($"Dialog execution failed: {ex.Message}", ex);
        }
    }

    private string GenerateDialogJson(
        DialogActionModule dialogModule,
        ExecutionSession session)
    {
        // Build dialog object based on type
        var dialog = new Dictionary<string, object?>
        {
            ["dialogId"] = dialogModule.Id,
            ["dialogName"] = dialogModule.Name,
            ["dialogType"] = dialogModule.DialogType.ToString(),
            ["description"] = dialogModule.Description
        };

        // Add content (message and help)
        var content = BuildContent(dialogModule, session);
        if (content.Count > 0)
        {
            dialog["content"] = content;
        }

        // Add prompt field (if applicable)
        if (ShouldShowPrompt(dialogModule))
        {
            dialog["prompt"] = BuildPrompt(dialogModule, session);
        }

        // Add list data (if LIST type)
        if (dialogModule.DialogType == DialogType.LIST && dialogModule.ListModuleId.HasValue)
        {
            dialog["listData"] = GetListData(dialogModule.ListModuleId.Value, session);
        }

        // Add options (if specified)
        if (dialogModule.OptionsFieldId.HasValue)
        {
            dialog["options"] = GetOptions(dialogModule.OptionsFieldId.Value, session);
        }

        // Add session context
        dialog["sessionId"] = session.SessionId;
        dialog["userId"] = session.UserId;

        return JsonSerializer.Serialize(dialog, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private Dictionary<string, object?> BuildContent(
    DialogActionModule dialogModule,
    ExecutionSession session)
    {
        var content = new Dictionary<string, object?>();

        // Main message (with field substitution)
        if (dialogModule.MessageFieldId.HasValue)
        {
            var messageValue = session.GetFieldValue(dialogModule.MessageFieldId.Value);
            if (messageValue != null)
            {
                // Substitute @FieldName in message text
                var message = SubstituteFieldReferences(messageValue.ToString(), session);
                content["message"] = message;
            }
        }

        // Help lines (with field substitution)
        var helpLines = new List<string>();

        if (dialogModule.Help1FieldId.HasValue)
        {
            var help = GetFieldValueAsString(dialogModule.Help1FieldId.Value, session);
            if (!string.IsNullOrEmpty(help))
                helpLines.Add(SubstituteFieldReferences(help, session));
        }

        if (dialogModule.Help2FieldId.HasValue)
        {
            var help = GetFieldValueAsString(dialogModule.Help2FieldId.Value, session);
            if (!string.IsNullOrEmpty(help))
                helpLines.Add(SubstituteFieldReferences(help, session));
        }

        if (dialogModule.Help3FieldId.HasValue)
        {
            var help = GetFieldValueAsString(dialogModule.Help3FieldId.Value, session);
            if (!string.IsNullOrEmpty(help))
                helpLines.Add(SubstituteFieldReferences(help, session));
        }

        if (helpLines.Count > 0)
        {
            content["helpLines"] = helpLines;
        }

        return content;
    }

    private object? BuildPrompt(
        DialogActionModule dialogModule,
        ExecutionSession session)
    {
        if (!dialogModule.ResultFieldId.HasValue)
            return null;

        var resultField = session.ModuleCache.GetModule(
            session.ApplicationId,
            dialogModule.ResultFieldId.Value) as FieldModule;

        if (resultField == null)
            return null;

        return new
        {
            fieldId = resultField.Id,
            fieldName = resultField.Name,
            fieldType = resultField.FieldType.ToString(),
            currentValue = session.GetFieldValue(resultField.Id),
            defaultValue = resultField.DefaultValue,
            masked = new
            {
                enabled = dialogModule.MaskInput,
                character = dialogModule.MaskCharacter
            }
        };
    }

    private object? GetListData(Guid listModuleId, ExecutionSession session)
    {
        var listModule = session.ModuleCache.GetModule(
            session.ApplicationId,
            listModuleId) as ListModule;

        if (listModule == null)
            return null;

        // Get list data from session (populated by DatabaseAction)
        var jsonData = session.GetFieldValue(listModule.Id) as string;

        if (string.IsNullOrEmpty(jsonData))
            return new { items = new List<object>(), maxRows = listModule.MaxRows };

        try
        {
            var items = JsonSerializer.Deserialize<List<object>>(jsonData);
            return new
            {
                items = items,
                maxRows = listModule.MaxRows
            };
        }
        catch
        {
            return new { items = new List<object>(), maxRows = listModule.MaxRows };
        }
    }

    private object? GetOptions(Guid optionsFieldId, ExecutionSession session)
    {
        var optionsText = GetFieldValueAsString(optionsFieldId, session);
        if (string.IsNullOrEmpty(optionsText))
            return null;

        // Parse: "F2:Next F3:Cancel" → [{value: "F2", text: "Next"}, ...]
        var options = new List<object>();
        var parts = optionsText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var colonIndex = part.IndexOf(':');
            if (colonIndex > 0)
            {
                options.Add(new
                {
                    value = part.Substring(0, colonIndex).Trim(),
                    text = part.Substring(colonIndex + 1).Trim()
                });
            }
        }

        return options;
    }

    private bool ShouldShowPrompt(DialogActionModule dialogModule)
    {
        return dialogModule.DialogType switch
        {
            DialogType.PROMPT => true,
            DialogType.LIST => true,
            DialogType.CONFIRM => false,
            DialogType.NOPROMPT => false,
            _ => true
        };
    }

    private string GetFieldValueAsString(Guid fieldId, ExecutionSession session)
    {
        var value = session.GetFieldValue(fieldId);
        return value?.ToString() ?? "";
    }

    private string SubstituteFieldReferences(string? text, ExecutionSession session)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Use FieldParser for @FieldName substitution (text mode, not SQL)
        return FieldParser.SubstituteFieldReferencesInText(text, session);
    }
}
